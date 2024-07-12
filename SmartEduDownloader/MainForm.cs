using System.Diagnostics;
using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace SmartEduDownloader;

public partial class MainForm : Form
{
    private IBrowserContext _Context;
    private readonly string _BooksRootFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "国家智慧教育平台_电子教材");
    private readonly string _NameString = "国家智慧教育平台 电子教材下载工具";
    private readonly string _VersionString = "Ver1.01 龙爸 友情提供";

    private readonly string _MyAvatarImageUrl = "https://pica.zhimg.com/3e1fa87c8ba8023679384796d1226a36_l.jpg?source=1def8aca";
    private readonly string _HomePageUrl = "https://basic.smartedu.cn/tchMaterial/";
    private readonly string _BookListPageUrl = "https://basic.smartedu.cn/tchMaterial?";
    private IPlaywright? _Playwright;
    private IBrowser _Browser;

    public MainForm()
    {
        InitializeComponent();
    }

    private async Task HandleBookListPage(IPage page)
    {
        if (!page.Url.StartsWith(_BookListPageUrl))
            return;

        // 等待 教材列表 加载完成
        await page.WaitForSelectorAsync(".index-module_item_GfOnF");
        // 列表页面，获取教材名称、详情地址
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
        img.src = '{_MyAvatarImageUrl}';
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
        span.textContent = '注意：点击下载，再点击查看已下载文件';
        line.appendChild(span);

        line = document.createElement('div');
        line.className = 'index-module_line_LgJAC';
        div.appendChild(line);

        span = document.createElement('span');
        span.textContent = '源代码：';
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
        // 对每一项
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
                    var bookUrl =
                        $"https://basic.smartedu.cn/tchMaterial/detail?contentType=assets_document&contentId={idString}&catalogType=tchMaterial&subCatalog=tchMaterial";
                    // 向同级元素中添加一个 <li> 元素
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
            a.textContent = '※ 下载电子版 ※';
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
        var url = $"https://r1-ndr.ykt.cbern.com.cn/edu_product/esp/assets_document/{idString}.pkg/pdf.pdf";
        WriteText($"点击下载：{name}, {idString}, {url}");

        //避免重复点击导致重复下载，先检查文件是否已存在
        var folder = ComposeFolderName(level, type, _BooksRootFolder);
        var fileName = Path.Combine(folder, $"{name}.pdf");
        if (File.Exists(fileName))
        {
            //如果文件已存在，则直接打开文件所在文件夹
            Process.Start("explorer.exe", folder);
            return $"已经下载：{fileName}";
        }
        //如果确认下载，则调用 DownloadBookFile 方法并显示在主窗口的下载列表中，以便打开查看。
        fileName = DownloadBookFile(name, url, folder);
        return $"下载完成：{fileName}";
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
            ViewportSize = new ViewportSize{ Height = 1080, Width = 1920},
        });
        // 暴露一个 C# 函数给页面的 JavaScript 环境
        await _Context.ExposeFunctionAsync<string, string, string, string, string>("handleLinkClick", OnJavaScriptCallback);
        // 监听上下文的 Page 事件
        _Context.Page += async (_, page) =>
        {
            // 监听事件，该事件在每次页面导航时触发
            page.FrameNavigated += async (ee, e) =>
            {
                await HandleBookListPage(page);
            };
        };
    }

    private async void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        await _Context.CloseAsync();
        await _Browser.CloseAsync();
    }
}