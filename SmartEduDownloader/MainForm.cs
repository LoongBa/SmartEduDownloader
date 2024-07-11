using System.Diagnostics;
using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace SmartEduDownloader;

public partial class MainForm : Form
{
    private readonly IBrowserContext _Context;
    private readonly string _BooksRootFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "国家智慧教育平台_电子教材");
    private readonly string _VersionString = "国家智慧教育平台 电子教材下载工具 ver 1.00 龙爸 友情提供";
    private readonly string _MyAvatarImageUrl = "https://pica.zhimg.com/3e1fa87c8ba8023679384796d1226a36_l.jpg?source=1def8aca";
    private readonly string _HomePageUrl = "https://basic.smartedu.cn/tchMaterial/";

    public MainForm()
    {
        InitializeComponent();

        var edgeUserDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft\\Edge\\User Data");
        var playwright = Playwright.CreateAsync().Result;
        // 启动持久上下文
        /*        var context = await playwright.Chromium.LaunchPersistentContextAsync(edgeUserDataPath, new BrowserTypeLaunchPersistentContextOptions
                {
                    Headless = false,
                    Channel = "msedge",
                });*/

        var browser = playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false,
            Channel = "msedge",
        }
        );
        _Context = browser.Result.NewContextAsync().Result;
        /*        await context.AddInitScriptAsync("window.open = function() { return null; };");
                await context.AddInitScriptAsync(@"
            document.addEventListener('DOMContentLoaded', function() {
                const links = document.querySelectorAll('a[target=""_blank""]');
                links.forEach(function(link) {
                    link.removeAttribute('target');
                });
            });
        ");*/
        // 监听上下文的 Page 事件
        _Context.Page += async (_, page) =>
        {
            // 为新页面订阅 Load 事件
            page.Load += async (_, __) =>
            {
                if (page.Url.StartsWith(_HomePageUrl))
                {// 教材详情页面
                    var id = Regex.Match(page.Url, @"contentId=([^&]+)").Groups[1].Value;
                    var url = $"https://r1-ndr.ykt.cbern.com.cn/edu_product/esp/assets_document/{id}.pkg/pdf.pdf";
                    await page.WaitForLoadStateAsync();
                    WriteText($"教材：{url}");
                }
                else
                {
                    //WriteText($"Load：{page.Url}");
                }
            };
            // 监听事件，该事件在每次页面导航时触发
            page.FrameNavigated += async (_, e) =>
            {
                if (page.Url.StartsWith("https://basic.smartedu.cn/tchMaterial?"))
                {// 列表页面，获取教材名称、详情地址
                    await page.WaitForLoadStateAsync();
                    // 注入脚本
                    var list = await page.QuerySelectorAllAsync(".index-module_item_GfOnF");
                    if (list.Count > 0)
                    {
                        await list[0].EvaluateAsync($@"
    element => {{
        var parent = element.parentElement;
        var li = document.createElement('li');
        li.className = 'index-module_item_GfOnF';
        parent.appendChild(li);

        var div = document.createElement('div');
        div.className = 'index-module_cover_DGT6P';
        li.appendChild(div);

        var img = document.createElement('img');
        img.src = '{_MyAvatarImageUrl}';
        img.style.width = '89px';
        img.style.height = '120px';
        div.appendChild(img);

        div = document.createElement('div');
        div.className = 'index-module_content_KmLzG';
        li.appendChild(div);

        var a = document.createElement('a');
        a.href = 'https://loongba.cn/Tools/SmartEduDownloader';
        a.textContent = '{_VersionString}';
        a.target = '_blank';
        div.appendChild(a);

        a = document.createElement('a');
        a.href = 'https://github.com/loongba/SmartEduDownloader';
        a.textContent = 'Github';
        a.target = '_blank';
        div.appendChild(a);

        a = document.createElement('a');
        a.href = 'https://gitee.com/loongba/SmartEduDownloader';
        a.textContent = 'Gitee';
        a.target = '_blank';
        div.appendChild(a);

    }}");
                    }
                    foreach (var li in list)
                    {
                        var levelTag = await page.QuerySelectorAsync(".index-module_current_qJFWH");
                        var level = await levelTag?.InnerTextAsync()!;
                        var tags = await page.QuerySelectorAllAsync(".fish-radio-tag-wrapper-checked");
                        var type = await tags[1].InnerTextAsync();
                        // 向下寻找 img 元素，获取教材封面
                        var img = await li.QuerySelectorAsync("img");
                        if (img != null)
                        {
                            // 获取 img 元素的 src 属性值，教材封面的 URL
                            var imgSrc = img.GetAttributeAsync("src").Result ?? string.Empty;
                            // 获取 IdString：cfdc0cbb-d23a-4b1d-ab28-a4d59b991dc6
                            var idString = Regex.Match(imgSrc, @"assets/(.*?)\.t/").Groups[1].Value;

                            // 向下寻找 ".index-module_line_LgJAC" 元素，获取教材名称
                            var nameTag = await li.QuerySelectorAsync(".index-module_line_LgJAC");
                            // 向 name div 中添加一个 <a> 链接，并为其添加点击事件监听器
                            // 在点击事件中传递参数给 handleLinkClick 函数
                            if (nameTag != null)
                            {
                                var bookName = await nameTag.InnerTextAsync();
                                var url =
                                    $"https://basic.smartedu.cn/tchMaterial/detail?contentType=assets_document&contentId={idString}&catalogType=tchMaterial&subCatalog=tchMaterial";
                                // 向同级元素中添加一个 <li> 元素
                                await li.EvaluateAsync($@"
    element => {{
        var parent = element.parentElement;
        var li = document.createElement('li');
        li.className = 'index-module_item_GfOnF';
        //li.textContent = '{bookName}';
        parent.appendChild(li);

        var div = document.createElement('div');
        div.className = 'index-module_cover_DGT6P';
        li.appendChild(div);

        const img = document.createElement('img');
        img.src = '{imgSrc}';
        img.style.width = '89px';
        img.style.height = '120px';
        div.appendChild(img);

        div = document.createElement('div');
        div.className = 'index-module_content_KmLzG';
        li.appendChild(div);

        var span = document.createElement('span');
        span.textContent = '《{bookName}》';
        div.appendChild(span);

        var a = document.createElement('a');
        a.href = 'javascript:handleLinkClick(""{bookName}"", ""{level}"", ""{type}"", ""{idString}"")';
        a.textContent = '下载教材电子版';
        div.appendChild(a);

        a = document.createElement('a');
        a.href = '{url}';
        a.target = '_blank';
        a.textContent = '在线查看';
        div.appendChild(a);

        parent.removeChild(element);
    }}"
                                );
                            }

                        }
                    }
                }
                else
                {
                    WriteText($"转到：{e.Url}");
                }
            };
            // 暴露一个 C# 函数给页面的 JavaScript 环境
            await page.ExposeFunctionAsync<string, string, string, string, string>("handleLinkClick", OnJavaScriptCallback);
        };
    }

    private string OnJavaScriptCallback(string name, string level, string type, string idString)
    {
        var url = $"https://r1-ndr.ykt.cbern.com.cn/edu_product/esp/assets_document/{idString}.pkg/pdf.pdf";
        WriteText($"链接被点击：{name}, {idString}, {url}");

        //避免重复点击导致重复下载，先检查文件是否已存在
        var folder = ComposeFolderName(level, type, _BooksRootFolder);
        var fileName = Path.Combine(folder, $"{name}.pdf");
        if (File.Exists(fileName))
        {
            //如果文件已存在，则直接打开文件所在文件夹
            Process.Start("explorer.exe", folder);
            return fileName;
        }

        //显示对话框，询问是否下载
        if (MessageBox.Show($@"是否下载：\r\n\t《{name}》？", @"下载确认", MessageBoxButtons.YesNo) == DialogResult.Yes)
        {
            //如果确认下载，则调用 DownloadBookFile 方法并显示在主窗口的下载列表中，以便打开查看。
            return DownloadBookFile(name, url, folder);
        }
        return string.Empty;
    }

    /// <summary>
    /// 下载指定的教材文件
    /// </summary>
    private string DownloadBookFile(string bookName, string url, string folder)
    {
        // 确保文件夹存在
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        // 下载文件
        var fileName = Path.Combine(folder, $"{bookName}.pdf");
        using var client = new HttpClient();
        // 用 HttpClient 下载文件
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
        await NavigateTo(_Context);
    }

    private async Task NavigateTo(IBrowserContext content)
    {
        var page = await content.NewPageAsync();
        await page.GotoAsync("https://basic.smartedu.cn/tchMaterial");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public void WriteText(string text)
    {
        // 检查是否需要跨线程调用
        if (txtLog.InvokeRequired)
        {
            // 使用 Invoke 方法在 UI 线程上执行操作
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

    private void Form1_Load(object sender, EventArgs e)
    {
        this.Text = _VersionString;
    }
}