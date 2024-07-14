using System.Diagnostics;
using System.Text;
using Microsoft.Playwright;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace SmartEduDownloader;

public partial class MainForm : Form
{
    private readonly string _DownloadFolder;
    private readonly string _NameString = "�����ǻ۽���ƽ̨ ���ӽ̲����ع���";
    private readonly string _VersionString = "Ver1.01 ���� �����ṩ";

    private const string MyAvatarImageUrl = "https://pica.zhimg.com/3e1fa87c8ba8023679384796d1226a36_l.jpg";
    private const string HomePageUrl = "https://basic.smartedu.cn/tchMaterial/";
    private const string BookListPageUrl = "https://basic.smartedu.cn/tchMaterial?";
    private IPlaywright? _Playwright;
    private IBrowser? _Browser;
    private IBrowserContext? _Context;
    private static readonly string _DownloadFileUrlPattern = "https://r1-ndr.ykt.cbern.com.cn/edu_product/esp/assets_document/{0}.pkg/pdf.pdf";

    public MainForm()
    {
        InitializeComponent();

        var downloadsPath = GetDownloadsPath();
        var folder = downloadsPath ?? string.Empty;
        _DownloadFolder = Path.Combine(folder, "_�����ǻ۽���ƽ̨_�̲ĵ��Ӱ�_");
        if (!Directory.Exists(_DownloadFolder))
            Directory.CreateDirectory(_DownloadFolder);
    }

    private async Task HandleBookListPage(IPage page)
    {
        if (!page.Url.StartsWith(BookListPageUrl))
            return;

        // �ȴ� �̲��б� �������
        await page.WaitForSelectorAsync(".index-module_item_GfOnF");
        // �б�ҳ�棬��ȡ�̲����ơ������ַ
        var list = await page.QuerySelectorAllAsync(".index-module_item_GfOnF");
        if (list.Count <= 0) return;

        await list[0].EvaluateAsync($@"
    element => {{
        const list = document.getElementById('LoongBaList');
        if (list) list.parentNode.removeChild(list);

        var parent = element.parentElement;
        var grand = parent.parentElement;
        var ul = document.createElement('ul');
        ul.id = 'LoongBaList';
        grand.appendChild(ul);

        var li = document.createElement('li');
        li.className = 'index-module_item_GfOnF';
        ul.appendChild(li);

        var div = document.createElement('div');
        div.className = 'index-module_cover_DGT6P';
        li.appendChild(div);

        var img = document.createElement('img');
        img.src = '{MyAvatarImageUrl}';
        img.style.width = '89px';
        img.style.height = '120px';
        div.appendChild(img);

        div = document.createElement('div');
        div.className = 'index-module_content_KmLzG';
        li.appendChild(div);

        var line = document.createElement('div');
        line.className = 'index-module_line_LgJAC';
        div.appendChild(line);

        var span = document.createElement('span');
        line.appendChild(span);
        span.textContent = '{_NameString}';

        line = document.createElement('div');
        line.className = 'index-module_line_LgJAC';
        div.appendChild(line);

        var a = document.createElement('a');
        a.href = 'https://loongba.cn/Tools/SmartEduDownloader';
        a.textContent = '{_VersionString}';
        a.style.color = '#ff6000';
        a.target = '_blank';
        line.appendChild(a);

        line = document.createElement('div');
        line.className = 'index-module_line_LgJAC';
        div.appendChild(line);
        
        span = document.createElement('span');
        span.textContent = 'ע�⣺������أ��ٵ���鿴�������ļ�';
        line.appendChild(span);

        line = document.createElement('div');
        line.className = 'index-module_line_LgJAC';
        div.appendChild(line);

        span = document.createElement('span');
        span.textContent = 'Դ���룺';
        line.appendChild(span);

        a = document.createElement('a');
        a.href = 'https://github.com/loongba/SmartEduDownloader';
        a.textContent = 'Github';
        a.style.color = '#ff6000';
        a.target = '_blank';
        span.appendChild(a);

        var spanText = document.createElement('span');
        spanText.innerHTML = '&nbsp;&nbsp;&nbsp;&nbsp;';
        spanText.style.color = '#ff6000';
        span.appendChild(spanText);

        a = document.createElement('a');
        a.href = 'https://gitee.com/loongba/SmartEduDownloader';
        a.textContent = 'Gitee';
        a.style.color = '#ff6000';
        a.target = '_blank';
        span.appendChild(a);
    }}");
        // ��ÿһ��
        foreach (var li in list)
        {
            var levelTag = await page.QuerySelectorAsync(".index-module_current_qJFWH");
            var level = await levelTag?.InnerTextAsync()!;
            var tags = await page.QuerySelectorAllAsync(".fish-radio-tag-wrapper-checked");
            var type = await tags[1].InnerTextAsync();
            // ����Ѱ�� img Ԫ�أ���ȡ�̲ķ���
            var img = await li.QuerySelectorAsync("img");
            if (img != null)
            {
                // ��ȡ img Ԫ�ص� src ����ֵ���̲ķ���� URL
                var imgSrc = img.GetAttributeAsync("src").Result ?? string.Empty;
                // ��ȡ IdString��cfdc0cbb-d23a-4b1d-ab28-a4d59b991dc6
                var idString = Regex.Match(imgSrc, @"assets/(.*?)\.t/").Groups[1].Value;

                // ����Ѱ�� ".index-module_line_LgJAC" Ԫ�أ���ȡ�̲�����
                var nameTag = await li.QuerySelectorAsync(".index-module_line_LgJAC");
                // �� name div �����һ�� <a> ���ӣ���Ϊ����ӵ���¼�������
                // �ڵ���¼��д��ݲ����� handleLinkClick ����
                if (nameTag != null)
                {
                    var bookName = await nameTag.InnerTextAsync();
                    var bookUrl =
                        $"https://basic.smartedu.cn/tchMaterial/detail?contentType=assets_document&contentId={idString}&catalogType=tchMaterial&subCatalog=tchMaterial";
                    // ��ͬ��Ԫ�������һ�� <li> Ԫ��
                    await li.EvaluateAsync($@"
    element => {{
        element.addEventListener('click', function(event) {{
            if (event.target.tagName === 'A' || event.target.closest('a')) {{
            }} else {{
                event.stopPropagation();
                event.preventDefault();
            }}
        }});

        var div = element.querySelector('.index-module_line_LgJAC');
        if(div) {{
            var parent = div.parentNode;
            var elements = parent.querySelectorAll('.LoongBa-class');
            if (elements) {{
                elements.forEach(e => {{
                    e.parentNode.removeChild(e);
                }});
            }}
            var line = document.createElement('div');
            if (div.parentNode)
                div.parentNode.insertBefore(line, div.nextSibling);
            line.className = 'index-module_line_LgJAC LoongBa-class';
            var a = document.createElement('a');
            a.href = 'javascript:void(0)';
            a.textContent = '�� ���ص��Ӱ� ��';
            a.style.color = '#ff6000';
            a.onclick = function(event) {{
                event.stopPropagation();
                handleLinkClick(""{bookName}"", ""{level}"", ""{type}"", ""{idString}"");
            }};
            line.appendChild(a);
        }}
    }}"
                    );
                }
            }
        }
    }

    private string OnJavaScriptCallback(string name, string level, string type, string idString)
    {
        var url = string.Format(_DownloadFileUrlPattern, idString);
        WriteText($"������أ�{name}, {idString}, {url}");

        //�����ظ���������ظ����أ��ȼ���ļ��Ƿ��Ѵ���
        var folder = ComposeFolderName(level, type, _DownloadFolder);
        var fileName = Path.Combine(folder, $"{name}.pdf");
        if (File.Exists(fileName))
        {
            //����ļ��Ѵ��ڣ���ֱ�Ӵ��ļ������ļ���
            Process.Start("explorer.exe", folder);
            return $"�Ѿ����أ�{fileName}";
        }
        //���ȷ�����أ������ DownloadBookFile ��������ʾ�������ڵ������б��У��Ա�򿪲鿴��
        fileName = DownloadBookFile(name, url, folder);
        return $"������ɣ�{fileName}";
    }

    /// <summary>
    /// ����ָ���Ľ̲��ļ�
    /// </summary>
    private string DownloadBookFile(string bookName, string url, string folder)
    {
        // ȷ���ļ��д���
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        // �����ļ�
        var fileName = Path.Combine(folder, $"{bookName}.pdf");
        using var client = new HttpClient();
        // �� HttpClient �����ļ�
        var response = client.GetAsync(url).Result;
        if (response.IsSuccessStatusCode)
        {
            using var stream = response.Content.ReadAsStreamAsync().Result;
            using var fileStream = File.Create(fileName);
            stream.CopyTo(fileStream);
        }
        return fileName;
    }

    private static string ComposeFolderName(string level, string type, string booksRootFolder)
    {
        var folder = Path.Combine(booksRootFolder, level);
        folder = Path.Combine(folder, type);
        return folder;
    }

    private async void btnNavigate_Click(object sender, EventArgs e)
    {
        if (_Context == null) return;
        await NavigateTo(_Context);
    }

    private async Task NavigateTo(IBrowserContext content)
    {
        var page = await content.NewPageAsync();
        // ����������Ӧ
        await page.RouteAsync("**/*", async (route) =>
        {
            var request = route.Request;
            var response = await request.ResponseAsync();
            var url = request.Url;

            // ��� URL �Ƿ�ƥ���ض��� JavaScript ����
            if (url.Contains("�ض���URL����"))
            {
                // �����Ӧ�����Ƿ�Ϊ JSON
                var contentType = response?.Headers["content-type"];
                if (contentType != null && contentType.Contains("application/json"))
                {
                    // ��ȡ JSON ��Ӧ����
                    var bytes = await response?.BodyAsync()!;
                    var jsonString = Encoding.UTF8.GetString(bytes);

                    // �����ﴦ�� JSON �ַ����������ȡ�ض�����
                    Console.WriteLine(jsonString);

                    // �����ʹ�� Json.NET (Newtonsoft.Json) �� System.Text.Json �������ʹ��� JSON ����
                }
            }

            // ����·������
            await route.ContinueAsync();
        });
        await page.GotoAsync("https://basic.smartedu.cn/tchMaterial");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public void WriteText(string text)
    {
        // ����Ƿ���Ҫ���̵߳���
        if (txtLog.InvokeRequired)
        {
            // ʹ�� Invoke ������ UI �߳���ִ�в���
            txtLog.Invoke(() =>
            {
                txtLog.AppendText(text + "\r\n");
            });
        }
        else
        {
            txtLog.AppendText(text);
        }
    }

    private async void MainForm_LoadAsync(object sender, EventArgs e)
    {
        Text = $@"{_NameString} {_VersionString}";

        var edgeUserDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft\\Edge\\User Data");
        _Playwright = await Playwright.CreateAsync();
        _Browser = await _Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false,
            Channel = "msedge",
        }
        );
        _Context = await _Browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Height = 1080, Width = 1920 },
        });
        // ��¶һ�� C# ������ҳ��� JavaScript ����
        await _Context.ExposeFunctionAsync<string, string, string, string, string>("handleLinkClick", OnJavaScriptCallback);
        // ���������ĵ� Page �¼�
        _Context.Page += (_, page) =>
        {
            // �����¼������¼���ÿ��ҳ�浼��ʱ����
            page.FrameNavigated += async (ee, e) =>
            {
                await HandleBookListPage(page);
            };
        };
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

    private async void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (_Context != null)
            await _Context.CloseAsync();
        if (_Browser != null)
            await _Browser.CloseAsync();
    }
}