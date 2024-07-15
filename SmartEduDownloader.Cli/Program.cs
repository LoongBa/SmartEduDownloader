using System.Drawing;
using PromptSharp;
using Console = Colorful.Console;
using System.Diagnostics;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using Formatter = Colorful.Formatter;

namespace SmartEduDownloader.Cli
{
    internal class Program
    {
        static async Task Main()
        {
            var downloadsPath = GetDownloadsPath();
            var folder = downloadsPath ?? string.Empty;
            _DownloadFolder = Path.Combine(folder, "_国家智慧教育平台_教材电子版_");
            if (!Directory.Exists(_DownloadFolder))
                Directory.CreateDirectory(_DownloadFolder);

            SayHello();
            await CheckAndDownloadData();
            await ShowMenuLevel();
        }

        const string NameString = "国家智慧教育平台 教材下载工具 Cli";
        const string VersionString = "Ver1.01 龙爸 友情提供";
        const string NameStringEnglish = "SmartEdu Downloader";
        const string VerStringEnglish = "Ver1.01 by LoongBa";
        const string AboutString = "陪伴孩子学习编程，支持开源请点 Star，谢谢";
        private const string HomePageUrl = "http://loongba.cn";
        const string Aria2CPath = "Aria2c\\aria2c.exe";     // aria2c 命令行工具的路径

        private static JArray? _books;
        private static JObject? _Catalog;
        static readonly List<string> _DataFiles =
        [
            "Data\\Part_100.json",
            "Data\\Part_101.json",
            "Data\\Part_102.json"
        ];
        private const string DataUrl =
            "https://s-file-1.ykt.cbern.com.cn/zxx/ndrs/resources/tch_material/version/data_version.json";

        private const string TagsDataUrl =
            "https://s-file-1.ykt.cbern.com.cn/zxx/ndrs/tags/tch_material_tag.json";
        private static readonly string _DownloadFileUrlPattern = "https://r1-ndr.ykt.cbern.com.cn/edu_product/esp/assets_document/{0}.pkg/pdf.pdf";
        private static string _DownloadFolder = string.Empty;

        private const string TagsFilename = "Data\\Catalog.json";
        private const string BooksDataFilename = "Data\\Books.json";
        //private const string MyAvatarImageUrl = "https://pica.zhimg.com/3e1fa87c8ba8023679384796d1226a36_l.jpg";

        static async Task CheckAndDownloadData()
        {
            // 获取 Catalog 数据文件
            await UpdateCatalogData();
            // 更新教材数据
            await UpdateBooksData();
        }

        /// <summary>
        /// 更新教材数据
        /// </summary>
        private static async Task DownloadBooksData()
        {
            try
            {
                // 下载数据文件 data_version.json
                var client = new HttpClient();
                var jsonString = await client.GetStringAsync(DataUrl);

                // 获取在线数据版本号，与本地数据版本号比较
                using var document = JsonDocument.Parse(jsonString);
                var root = document.RootElement;
                var urls = root.GetProperty("urls").GetString();
                if (urls == null)
                    Console.WriteLine("注意：数据文件列表为空。", Color.Red);
                else
                {
                    // 提取 urls 中的数据文件 url 列表
                    var urlList = urls.Split(',');
                    for (var i = 0; i < urlList.Length; i++)
                    {
                        var url = urlList[i];
                        var filename = Path.GetFileName(url);
                        filename = Path.Combine("Data", filename);
                        // 如果数据文件不存在，则下载数据文件
                        if (!File.Exists(filename))
                        {
                            if (await DownloadFileLite(url, filename))
                            {
                                _DataFiles[i] = filename; // 更新数据文件名列表
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"DownloadBooksData() 下载 data_version 数据文件出错：{e}", Color.Red);
            }
        }

        /// <summary>
        /// 更新教材数据
        /// </summary>
        private static async Task UpdateBooksData()
        {
            try
            {
                System.Console.ForegroundColor = ConsoleColor.Cyan;
                System.Console.Write("正在更新教材数据...");
                // 读取，或更新 Books 数据文件
                if (File.Exists(BooksDataFilename))
                    _books = JArray.Parse(await File.ReadAllTextAsync(BooksDataFilename));
                else
                {
                    // 下载 books 数据文件
                    await DownloadBooksData();
                    // 合并数据文件
                    _books = new JArray();
                    foreach (var dataFile in _DataFiles)
                    {
                        // 合并数据文件中的数据，剔除无效部分
                        var jsonArray = JArray.Parse(await File.ReadAllTextAsync(dataFile));
                        {
                            foreach (var token in jsonArray)
                            {
                                var book = new JObject
                                {
                                    { "id", token["id"] },
                                    { "title", token["title"] },
                                    { "label", token["label"]?[1]!.ToString().Replace(" ","·") },
                                    { "tag_paths", token["tag_paths"]?[0] },
                                };
                                _books.Add(book);
                            }
                            File.Delete(dataFile); // 删除已合并的数据文件
                        }
                    }
                    // 写入合并后的数据文件
                    await File.WriteAllTextAsync(BooksDataFilename, _books.ToString());
                }
                System.Console.Write("\r更新教材数据...完成。\n");
                System.Console.ResetColor();
            }
            catch (Exception e)
            {
                ShowErrorMessage($"UpdateBooksData() 更新 books 数据文件出错：{e}");
            }
        }

        /// <summary>
        /// 获取 Catalog 数据文件
        /// </summary>
        private static async Task UpdateCatalogData()
        {
            try
            {
                System.Console.ForegroundColor = ConsoleColor.Cyan;
                System.Console.Write("正在更新教材目录数据...");
                // 读取，或更新 Catalog 数据文件
                if (File.Exists(TagsFilename))
                    _Catalog = JObject.Parse(await File.ReadAllTextAsync(TagsFilename));
                else
                {
                    // 下载 tags 数据文件
                    await DownloadFileLite(TagsDataUrl, TagsFilename);
                    // 简化 tags
                    var jsonString = await File.ReadAllTextAsync(TagsFilename);
                    _Catalog = JObject.Parse(jsonString);
                    HashSet<string> propertiesToKeep = ["tag_id", "tag_name", "children", "hierarchies"]; // 定义需要保留的属性名
                    FilterProperties(_Catalog, propertiesToKeep); // 递归处理每个元素
                    // 保存 Catalog 数据文件
                    await File.WriteAllTextAsync(TagsFilename, _Catalog.ToString());
                }
                System.Console.Write("\r更新教材目录数据...完成。\n");
                System.Console.ResetColor();
            }
            catch (Exception e)
            {
                ShowErrorMessage($"\rUpdateCatalogData() 更新 tags 数据文件出错：{e}");
            }
        }

        private static void ShowErrorMessage(string message)
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine(message);
            System.Console.ResetColor();
        }

        static void FilterProperties(JToken token, HashSet<string> propertiesToKeep)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    var toRemove = new List<JProperty>();
                    foreach (var prop in ((JObject)token).Properties())
                    {
                        if (!propertiesToKeep.Contains(prop.Name.ToLower()))
                        {
                            toRemove.Add(prop);
                        }
                        else
                        {
                            if (prop.Value.Type == JTokenType.Array)
                            {
                                // 移除空数组
                                if (!prop.HasValues)
                                    toRemove.Add(prop);
                                else if (prop.Name.Equals("hierarchies", StringComparison.OrdinalIgnoreCase))
                                {
                                    var first = prop.Value.First;
                                    if (first?["children"] != null && !first["children"]!.HasValues)
                                        toRemove.Add(prop);
                                    else
                                        FilterProperties(prop.Value, propertiesToKeep);
                                }
                                else
                                    FilterProperties(prop.Value, propertiesToKeep);
                            }
                            else
                                FilterProperties(prop.Value, propertiesToKeep);
                        }
                    }
                    // 移除不需要的属性
                    foreach (var prop in toRemove)
                    {
                        prop.Remove();
                    }
                    break;
                case JTokenType.Array:
                    foreach (var child in token.Children())
                    {
                        FilterProperties(child, propertiesToKeep);
                    }
                    break;
            }
        }

        /// <summary>
        /// 轻量下载
        /// </summary>
        static async Task<bool> DownloadFileLite(string downloadUrl, string filename)
        {
            if (downloadUrl == null) throw new ArgumentNullException(nameof(downloadUrl));
            if (filename == null) throw new ArgumentNullException(nameof(filename));

            var folder = Path.GetDirectoryName(filename);
            if (folder == null) return false;

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            // 使用 HttpClient 下载文件
            var client = new HttpClient();
            using var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            // 保存为文件
            await using var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);
            await response.Content.CopyToAsync(fileStream);

            return true;
        }

        /// <summary>
        /// 使用 aria2c 下载文件
        /// </summary>
        static async Task<string> DownloadFile(string downloadUrl, string directory, string filename)
        {
            if (downloadUrl == null) throw new ArgumentNullException(nameof(downloadUrl));
            if (filename == null) throw new ArgumentNullException(nameof(filename));

            // 设置aria2c的参数
            var arguments = $"-c -x 10 -s 10 --console-log-level=info --summary-interval=1 \"{downloadUrl}\" -d \"{directory}\" -o \"{filename}\"";

            // 创建 ProcessStartInfo
            var startInfo = new ProcessStartInfo()
            {
                FileName = Aria2CPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };

            // 创建并启动进程
            using var process = new Process();
            process.StartInfo = startInfo;
            process.Start();

            // 异步读取标准输出流，显示下载进度
            while (!process.StandardOutput.EndOfStream)
            {
                var line = await process.StandardOutput.ReadLineAsync();
                var progressRegex = new Regex(@"\b(\d+)%");
                if (line != null)
                {
                    var match = progressRegex.Match(line);
                    if (match.Success)
                    {
                        // 提取并显示下载进度百分比
                        var progress = match.Groups[1].Value;
                        Console.Write($"\r下载: {progress}% {filename}", Color.LightGreen);
                    }
                }
            }

            await process.WaitForExitAsync();
            return filename;
        }

        private static OrderedDictionary<string, string> LoadDictionaryFromCatalog()
        {
            return LoadDictionaryFromCatalog(string.Empty);
        }

        private static OrderedDictionary<string, string> LoadDictionaryFromCatalog(string tag_Id)
        {
            var dictionary = new OrderedDictionary<string, string>();
            // 使用 JSONPath 表达式来查找具有特定 tag_id 的对象
            var path = $"$.hierarchies..children[?(@.tag_id == '{tag_Id}')].hierarchies[0].children";
            if (string.IsNullOrEmpty(tag_Id))
                path = "$.hierarchies[0].children[0].hierarchies[0].children";
            var children = _Catalog!.SelectToken(path);
            if (children == null) return dictionary;    //TODO: 提示：未找到对应的 tag_id

            foreach (var token in children)
            {
                var name = token.SelectToken("$.tag_name")?.ToString();
                var id = token.SelectToken("$.tag_id")?.ToString();
                if (name != null && name.Contains("•")) name = name.Replace("•", "·");
                if (id != null && name != null)
                    dictionary.Add(name, id);
            }
            return dictionary;
        }

        private static List<JToken> LoadBooks(string subjectId, string typeId, string gradeId)
        {
            var path = $"$.[?(@.tag_paths=~ /.*{subjectId}\\/{typeId}\\/{gradeId}.*/i)]";
            var books = _books!.SelectTokens(path).ToList();
            return books;
        }

        static async Task ShowMenuLevel()
        {
            var dictionary = LoadDictionaryFromCatalog();  // 学段列表
            // 使用自定义比较器进行排序
            var sortOrder = new List<string> { "小学", "初中", "高中", "小学（五·四学制）", "初中（五·四学制）" }; // 指定排序顺序   
            var levels = dictionary.Keys.ToArray();
            Array.Sort(levels, (x, y) => sortOrder.IndexOf(x).CompareTo(sortOrder.IndexOf(y)));
            var menuItems = new List<string>(levels);
            AddExtraMenuItem(menuItems, true);

            var levelName = Prompt.Select("请选择需下载教材的 【学段】", menuItems);
            switch (levelName)
            {
                case "↑ 返回上级":
                    SayHello();
                    break;
                case "← 退出":
                    SayGoodbye();
                    break;
                default:
                    var levelId = dictionary[levelName];
                    await ShowMenuSubject(levelName, levelId);
                    break;
            }
        }

        static async Task ShowMenuSubject(string levelName, string levelId)
        {
            var dictionary = LoadDictionaryFromCatalog(levelId);
            var menuItems = new List<string>(dictionary.Keys);
            AddExtraMenuItem(menuItems);

            var subjectName = Prompt.Select($"请选择【{levelName}】教材的【科目】", menuItems);
            switch (subjectName)
            {
                case "↑ 返回上级":
                    await ShowMenuLevel();
                    break;
                case "← 退出":
                    SayGoodbye();
                    break;
                default:
                    var subjectId = dictionary[subjectName];
                    await ShowMenuType(levelName, levelId, subjectName, subjectId);
                    break;
            }
        }

        static async Task ShowMenuType(string levelName, string levelId, string subjectName, string subjectId)
        {
            var dictionary = LoadDictionaryFromCatalog(subjectId);  // 学段列表
            var menuItems = new List<string>(dictionary.Keys);
            AddExtraMenuItem(menuItems);

            var typeName = Prompt.Select($"请选择【{subjectName}】的【版本】", menuItems);
            switch (typeName)
            {
                case "↑ 返回上级":
                    await ShowMenuSubject(levelName, levelId);
                    break;
                case "← 退出":
                    SayGoodbye();
                    break;
                default:
                    var typeId = dictionary[typeName];
                    await ShowMenuGrade(levelName, levelId, subjectName, subjectId, typeName, typeId);
                    break;
            }
        }

        static async Task ShowMenuGrade(string levelName, string levelId, string subjectName, string subjectId, string typeName,
            string typeId)
        {
            // 一年级 到 九年级 的数组，用于排序
            var sortOrder = new List<string> { "一年级", "二年级", "三年级", "四年级", "五年级", "六年级", "七年级", "八年级", "九年级", "※ 下载全部" };
            var dictionary = LoadDictionaryFromCatalog(typeId);
            var grades = dictionary.Keys.ToArray();
            Array.Sort(grades, (x, y) => sortOrder.IndexOf(x).CompareTo(sortOrder.IndexOf(y)));
            var menuItems = new List<string>(grades) { "※ 下载全部" };
            AddExtraMenuItem(menuItems);

            var gradeName = Prompt.Select($"请选择【{levelName}·{subjectName}·{typeName}】的【年级】", menuItems);
            switch (gradeName)
            {
                case "↑ 返回上级":
                    await ShowMenuType(levelName, levelId, subjectName, subjectId);
                    break;
                case "← 退出":
                    SayGoodbye();
                    break;
                case "※ 下载全部":
                    await ConfirmDownloadAll(levelName, levelId, subjectName, subjectId, typeName, typeId, dictionary);
                    break;
                default:
                    var gradeId = dictionary[gradeName];
                    await ConfirmDownload(levelName, levelId, subjectName, subjectId, typeName, typeId, gradeName, gradeId);
                    break;
            }
        }

        private static async Task ConfirmDownloadAll(string levelName, string levelId, string subjectName, string subjectId, string typeName, string typeId, OrderedDictionary<string, string> grades)
        {
            Formatter[] formatters =
            [
                new(levelName, Color.Red),
                new(subjectName, Color.Orange),
                new(typeName, Color.LightGreen),
                new Formatter("※ 全部年级 ※", Color.Red)
            ];
            Console.WriteLineFormatted("下载【{0}·{1}·{2}】{3} 的教材：", Color.Gray, formatters);
            var ok = Prompt.Confirm($"请确认是否下载 ※ 全部年级 ※ 的教材？");
            if (!ok)
            {
                await ShowMenuGrade(levelName, levelId, subjectName, subjectId, typeName, typeId);
            }
            else
            {
                foreach (var grade in grades)
                {
                    var gradeName = grade.Key;
                    var gradeId = grade.Value;
                    var folder = Path.Combine(_DownloadFolder, $"{levelName}\\{subjectName}\\{typeName}\\{gradeName}");
                    var books = LoadBooks(subjectId, typeId, gradeId);
                    await downloadBooks(books, folder); // 下载教材列表
                }
            }
            ok = Prompt.Confirm($"是否继续下载其它教材？否则退出");
            if (ok)
                await ShowMenuSubject(levelName, levelId);
            else
                SayGoodbye();
        }

        private static async Task downloadBooks(List<JToken> books, string folder)
        {
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            foreach (var book in books)
            {
                var bookName = book["title"];
                var bookId = book["id"];
                var filename = $"{bookName}.pdf";
                var downloadUrl = string.Format(_DownloadFileUrlPattern, bookId);
                filename = await DownloadFile(downloadUrl, folder, filename);
                System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.Write($"\r下载成功：{filename}\n");
                System.Console.ResetColor();
            }
        }

        private static async Task ConfirmDownload(string levelName, string levelId, string subjectName, string subjectId,
                string typeName, string typeId, string gradeName, string gradeId)
        {
            Formatter[] formatters =
            [
                new(levelName, Color.Red),
                new(subjectName, Color.Orange),
                new(typeName, Color.LightGreen),
                new(gradeName, Color.Red),
            ];
            Console.WriteLineFormatted("【{0}·{1}·{2}·{3}】的教材有：", Color.Gray, formatters);
            var count = 0;
            // 查询教材列表
            var books = LoadBooks(subjectId, typeId, gradeId);
            foreach (var book in books)
            {
                var bookName = book["title"];
                Console.WriteLine($"\t{++count:D2} {bookName}");
            }

            var ok = Prompt.Confirm($"请确认是否下载全部？");
            if (!ok)
            {
                await ShowMenuGrade(levelName, levelId, subjectName, subjectId, typeName, typeId);
                return;
            }
            var folder = Path.Combine(_DownloadFolder, $"{levelName}\\{subjectName}\\{typeName}\\{gradeName}");
            await downloadBooks(books, folder); // 下载教材列表

            ok = Prompt.Confirm($"是否继续下载其它教材？否则退出");
            if (ok)
                await ShowMenuGrade(levelName, levelId, subjectName, subjectId, typeName, typeId);
            else
                SayGoodbye();
        }

        private static void AddExtraMenuItem(List<string> menuItems, bool topLevel = false)
        {
            if (!topLevel)
                if (!menuItems.Contains("↑ 返回上级"))
                    menuItems.Add("↑ 返回上级");
            if (!menuItems.Contains("← 退出"))
                menuItems.Add("← 退出");
        }

        private static void SayHello()
        {
            Console.WriteAscii(NameStringEnglish, Color.Green);
            Console.WriteAscii(VerStringEnglish, Color.Chocolate);
            Console.Write($"欢迎使用 {NameString}", Color.Chocolate);
            Console.WriteLine($"\t{VersionString}", Color.Orange);
            Console.Write($"{AboutString}", Color.Chocolate);
            Console.WriteLine($"\t{HomePageUrl}", Color.Orange);
            
            System.Console.ResetColor();
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.BackgroundColor= ConsoleColor.White;
            Console.WriteLine($"※ 请用键盘 上↑ 下↓ 键选择，按 回车 确定 ※ ", Color.LightGreen);
            System.Console.ResetColor();
            System.Console.ForegroundColor = ConsoleColor.White;
        }

        private static void SayGoodbye()
        {
            Console.WriteLine("\r\n感谢使用，再见！", Color.Chocolate);
            Console.WriteAscii("Good Bye!", Color.Chocolate);
            Console.WriteLine($"下载目录：{_DownloadFolder}", Color.Chocolate);
            // 用 explorer 打开下载目录 _DownloadFolder
            Process.Start("explorer.exe", _DownloadFolder);
            // 等待用户按任意键
            System.Console.ReadKey();

            Environment.Exit(0);
        }

        [DllImport("shell32.dll")]
        private static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr pszPath);
        public static string? GetDownloadsPath()
        {
            var downloadsFolderGuid = new Guid("374DE290-123F-4565-9164-39C4925E467B"); // GUID for Downloads folder
            SHGetKnownFolderPath(downloadsFolderGuid, 0, IntPtr.Zero, out var pszPath);
            var path = Marshal.PtrToStringUni(pszPath);
            Marshal.FreeCoTaskMem(pszPath);
            return path;
        }
    }
    public class OrderedDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> where TKey : notnull
    {
        private readonly Dictionary<TKey, TValue> dictionary = new();
        private readonly List<TKey> keys = new();
        public IEnumerable<TKey> Keys => keys.AsReadOnly();

        public void Add(TKey key, TValue value)
        {
            dictionary.Add(key, value);
            keys.Add(key);
        }

        public TValue this[TKey key] => dictionary[key];

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var key in keys)
            {
                yield return new KeyValuePair<TKey, TValue>(key, dictionary[key]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
