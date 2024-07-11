using System.Diagnostics;
using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace SmartEduDownloader;

public partial class MainForm : Form
{
    private readonly IBrowserContext _Context;
    private readonly string _BooksRootFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "�����ǻ۽���ƽ̨_���ӽ̲�");
    private readonly string _VersionString = "�����ǻ۽���ƽ̨ ���ӽ̲����ع��� ver 1.00 ���� �����ṩ";
    private readonly string _MyAvatarImageUrl = "https://pica.zhimg.com/3e1fa87c8ba8023679384796d1226a36_l.jpg?source=1def8aca";
    private readonly string _HomePageUrl = "https://basic.smartedu.cn/tchMaterial/";

    public MainForm()
    {
        InitializeComponent();

        var edgeUserDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft\\Edge\\User Data");
        var playwright = Playwright.CreateAsync().Result;
        // �����־�������
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
        // ���������ĵ� Page �¼�
        _Context.Page += async (_, page) =>
        {
            // Ϊ��ҳ�涩�� Load �¼�
            page.Load += async (_, __) =>
            {
                if (page.Url.StartsWith(_HomePageUrl))
                {// �̲�����ҳ��
                    var id = Regex.Match(page.Url, @"contentId=([^&]+)").Groups[1].Value;
                    var url = $"https://r1-ndr.ykt.cbern.com.cn/edu_product/esp/assets_document/{id}.pkg/pdf.pdf";
                    await page.WaitForLoadStateAsync();
                    WriteText($"�̲ģ�{url}");
                }
                else
                {
                    //WriteText($"Load��{page.Url}");
                }
            };
            // �����¼������¼���ÿ��ҳ�浼��ʱ����
            page.FrameNavigated += async (_, e) =>
            {
                if (page.Url.StartsWith("https://basic.smartedu.cn/tchMaterial?"))
                {// �б�ҳ�棬��ȡ�̲����ơ������ַ
                    await page.WaitForLoadStateAsync();
                    // ע��ű�
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
                                var url =
                                    $"https://basic.smartedu.cn/tchMaterial/detail?contentType=assets_document&contentId={idString}&catalogType=tchMaterial&subCatalog=tchMaterial";
                                // ��ͬ��Ԫ�������һ�� <li> Ԫ��
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
        span.textContent = '��{bookName}��';
        div.appendChild(span);

        var a = document.createElement('a');
        a.href = 'javascript:handleLinkClick(""{bookName}"", ""{level}"", ""{type}"", ""{idString}"")';
        a.textContent = '���ؽ̲ĵ��Ӱ�';
        div.appendChild(a);

        a = document.createElement('a');
        a.href = '{url}';
        a.target = '_blank';
        a.textContent = '���߲鿴';
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
                    WriteText($"ת����{e.Url}");
                }
            };
            // ��¶һ�� C# ������ҳ��� JavaScript ����
            await page.ExposeFunctionAsync<string, string, string, string, string>("handleLinkClick", OnJavaScriptCallback);
        };
    }

    private string OnJavaScriptCallback(string name, string level, string type, string idString)
    {
        var url = $"https://r1-ndr.ykt.cbern.com.cn/edu_product/esp/assets_document/{idString}.pkg/pdf.pdf";
        WriteText($"���ӱ������{name}, {idString}, {url}");

        //�����ظ���������ظ����أ��ȼ���ļ��Ƿ��Ѵ���
        var folder = ComposeFolderName(level, type, _BooksRootFolder);
        var fileName = Path.Combine(folder, $"{name}.pdf");
        if (File.Exists(fileName))
        {
            //����ļ��Ѵ��ڣ���ֱ�Ӵ��ļ������ļ���
            Process.Start("explorer.exe", folder);
            return fileName;
        }

        //��ʾ�Ի���ѯ���Ƿ�����
        if (MessageBox.Show($@"�Ƿ����أ�\r\n\t��{name}����", @"����ȷ��", MessageBoxButtons.YesNo) == DialogResult.Yes)
        {
            //���ȷ�����أ������ DownloadBookFile ��������ʾ�������ڵ������б��У��Ա�򿪲鿴��
            return DownloadBookFile(name, url, folder);
        }
        return string.Empty;
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

    private void Form1_Load(object sender, EventArgs e)
    {
        this.Text = _VersionString;
    }
}