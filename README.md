# SmartEdu Downloader Cli v1.2

# 一、中小学教材电子版下载器 极简版——命令行版本 V1.2

## 告别诱导关注，远离低质量电子版

+ **质量保证**，教材来自 **国家平台和各大出版社**，**可复制文本和图片**的**真 PDF**，不是扫描和拍照可比较；

+ **工具极简**，**最小不到 3M**（另外一个版本只有一行 JS 代码），没有复杂的要求；

+ **操作极简**，免注册、免登录，不需要复制教材网页地址，（另外有个**增强版**可下载课件视频，待发布）；

+ **批量下载**，偷懒的话，选择好学段、科目和版本类型，即可 **一次性下载所有年级** 的教材；

+ **方便学习**，**代码开源、免费无套路**，原本就是带我家小朋友学习编程的练手项目，**一起学习，共同进步**；

## 项目源代码：

- Github: [GitHub - LoongBa/SmartEduDownloader: 下载 国家智慧教育平台 电子教材、视频课程的极简工具。这是一个陪孩子学编程的练习项目，将陆续练习 WinForm、WPF、MAUI 等 UI 实现方式。](https://github.com/LoongBa/SmartEduDownloader)

- Gitee：[SmartEduDownloader: 下载 国家智慧教育平台 电子教材、视频课程的极简工具。这是一个陪孩子学编程的练习项目，将陆续练习 WinForm、WPF、MAUI 等 UI 实现方式。](https://gitee.com/LoongBa/SmartEduDownloader)

## 0. 更新说明：

### **Ver1.2** 终于，官网在 2024.08.31 更新了 2024版教材，同时系统略有修改。

+ 修改下载逻辑，支持下载 2024 新版本教材；

+ 菜单支持分页：部分科目（如英语）不同出版社太多，在窗口太小的情况下可能导致程序错误，因此为菜单增加了分页，请按左右箭头切换菜单。
  
  ——回头改进菜单项序号提示 ^}^

+ 已知问题：官方尚未更新部分教材，如英语 PEP 一年级。
  
  ——过段时间应该就有了。

> 注意：仅有部分教材发布了基于2022年审定的新版。未出新版的教材，出版社仅对旧版加了水印，内容未变，请自行鉴别。

<img src="./images_README/8c50b37a76072d022b5d95795571952e41607589.png" title="" alt="" width="945">

### Ver1.1

+ 加入了版本检测，为尽量避免阻塞操作，退出时运行并提示（仅提示）；

+ 加入了检测 开源免费的下载工具 `Aria2c`，如果已经安装则忽略，否则自动下载最新 1.37 版——有的教材文件略大，批量下载时使用 `Aria2c` 会更高效；

+ **关闭了 AOT 编译优化**，发布的 exe 反而小了许多，估计是对 AOT 的理解和设置不对，否则应该相反才对。

## 1. 下载说明

1. **Windows 11** 的朋友可以下载更小的版本： **压缩包小于 400KB，可执行文件 1MB 左右**
   **【最简版】** SmartEduDownloader.Cli_V1.2_Runtime_forWin11_x64.zip
   （Win11 较新版已经内置 .NET 8 运行环境）

2. **Windows 10** 的朋友可下载上面的版本，如果系统提示安装 .NET 8 运行环境，根据提示操作即可（第一次）

3. **Windows 7** 的朋友请试试看上面的版本，不行的话请下载： **【独立版】** SmartEduDownloader.Cli_V1.2_Standalone_forWin10_x64.zip
   （不依赖 .NET 8 运行环境的版本，在 Windows 7 下也可以运行——我没测试环境，**欢迎反馈**）

### 1.1 运行环境的依赖情况说明

1. **【独立版】** 不需要安装 .NET 8 运行环境，但压缩包略大：**7.8MB** 左右。 

2. **【最简版】** Windows 11 内置 .NET 8 运行环境，Windows 7/10 可能系统会提示下载安装 .NET 8 运行环境，或自行安装：
   
   + 访问 **微软官网** 选择下载最新版本：[https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0](https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0) （**右下角下载【运行时】 x64 版本，安装**）
     
     或者
   
   + 从 **微软官网** 直接下载 **.NET 8 runtime 8.07 x64**：[https://download.visualstudio.microsoft.com/download/pr/3980ab0a-379f-44a0-9be6-eaf74c07a3b3/bd1cc6107ff3d8fe0104d30f01339b74/dotnet-runtime-8.0.7-win-x64.exe](https://download.visualstudio.microsoft.com/download/pr/3980ab0a-379f-44a0-9be6-eaf74c07a3b3/bd1cc6107ff3d8fe0104d30f01339b74/dotnet-runtime-8.0.7-win-x64.exe)  

**其它系统如 macOS、32位 Windows**，我没测试过，但 **.NET 跨平台，需要的话发布时增加不同版本** 是很简单的。

但从下载教材的需求来说，想必不太有需求，将就解决一下吧 &#128514;  

## 2. 已知问题

1. 分别在 `CMD` 和 `Powershell` 运行时，**字符颜色不同**，影响不大，等有空再仔细检查原因；
2. **默认下载**到 “**我的下载**” 目录下的【_国家智慧教育平台_教材电子版_】目录（每个人的设置不同，退出前会显示保存位置，并自动打开该目录），
   ——极简版的用户**自行设置目标目录**的意义不太大，故**简化之**
3. **目录结构**暂时不支持选择，默认为：学段——科目——版本（部编版等）——年级，
   ——**考虑抽空升级，允许选择不同的目录结构**，如：版本——学段——科目——年级
4. **尚未测试 .NET AOT 编译优化**，可执行文件应该还可以更小。

---

## 3. 其它编程练手项目

在孩子去年到今年学习编程过程中，针对**中小学教材电子版的下载**，另外还有两个练手版本：  

1. **极简中的极简 JS 版：** Javascript，**只需将一句 js 代码复制到浏览器地址栏**（或者 F12 开发工具的控制台），就能下载当前页面的电子教材  
   **——添加到收藏夹，非常方便  
   ——缺点是不支持一次下载多个教材**
   **Github:** [GitHub - LoongBa/SmartEduDownloaderJS: 极简一键下载【国家智慧教育平台电子教材】，无需注册、登录，无需下载安装软件和环境，粘贴一行 Javascript 即可。](https://github.com/LoongBa/SmartEduDownloaderJS)
   
   **Gitee：** [SmartEduDownloaderJS: 极简一键下载【国家智慧教育平台电子教材】，无需注册、登录，无需下载安装软件和环境，在浏览器粘贴一行 Javascript 即可。](https://gitee.com/LoongBa/SmartEduDownloaderJS)

2. **增强版**：——有极简版，暂时没发布，考虑抽空加入**下载视频教程的全套素材**再发布（视频、教案PPT等）  

**请教各位一个问题：有【下载视频教程】的需求么？**  
**——如果有需求，抽空加上这个功能**

---

## 4. 使用说明

> 用键盘上下键选择，
> 
> 用回车键确定，
> 
> 确认时输入“y”，
> 
> 完事。

<img title="" src="./images_README/6818bdc8dbab3b8f8864623c019f7d1cf9288e53.jpg" alt="教材下载器_极简版.png" width="1165">

# 二、背景信息：免费电子版教材的来源

## 1. 义务教育阶段免费提供电子版教材

**无论哪个出版社**的教材，**所有涉及九年制义务教育的教材**，按照国家《**中华人民共和国义务教育法**》之规定，

以及2020年2月14日教育部《[关于发布中小学国家课程教材电子版链接的通告](https://www.gov.cn/xinwen/2020-02/14/content_5478551.htm)》

将各中小学教材编写出版单位提供的**免费电子版教材**链接统一予以公布，才有了下载电子版教材之说。

## 2. 人教社：人民教育出版社 一直免费提供

多年以来，人教社一直免费提供教材电子版在线阅读和下载。

我一直从[人民教育出版社的官网](http://www.pep.com.cn/)下载小学课本电子版——因为没用到其它教材，就没关心其它版本。

后来大概是在2022年，**数学教材问题**引发重新修改四年级以后的数学教材，人教社官网上暂时不再提供涉及调整的教材。

恢复后，教材地址从 [人民教育出版社官方网站－培根铸魂　启智增慧](http://bp.pep.com.cn/jc) 改为了现在可用的 [https://jc.pep.com.cn/](https://jc.pep.com.cn/) ，

——但仅提供在线阅读，暂时没看到下载链接。

（之前陪孩子学习 Python 时，写了一个工具：从人教社直接下载教材的高清图片，然后合成为 PDF）

## 3. 国家智慧教育平台：国家智慧教育公共服务平台、国家中小学智慧教育平台

此后，同期由**教育部教育技术与资源发展中心**（**中央电化教育馆**）主导建设维护的**国家智慧教育公共服务平台**也多次改版、完善，

收录了包括人民教育出版社在内的**多家出版单位的电子教材**，成为更完整、权威的资源平台。这里对该平台暂不展开介绍。

**国家智慧教育平台** 全称：【[国家智慧教育公共服务平台](https://www.smartedu.cn/)】。

其中，**中小学内容部分**又叫做【[国家中小学智慧教育平台](https://basic.smartedu.cn/)】。

——这是目前最全、最方便的教材下载来源。

——还有非常多不错的学习资料，是教师备课、家长战友们和孩子们学习的宝藏。

即便**使用本工具不需要注册、登录**，

——毕竟影响了该平台的推广和发展，白白占用了人家的资源，还是尽量支持一下，何况**平台有不少优质的教学资源**。

> 关于【智慧教育】这个话题，以及如何系统地理解和掌握学习的方法论，回头我再细细分享。

## 4. 推荐：人教社小程序

之所以再次提到人教社，因为从家长和学生角度，**人教社公众号和小程序更好用**——**公益、免费**：

1. **方便手机、平板**等移动设备查看和使用；

2. **小程序支持点读**：语文、英语 都支持 “**指到哪里读哪里**”，妈妈再也不用担心我的学习了。

作为家长和学生，真心**感谢国家、感谢教育部、感谢相关出版机构**。

## 5. 告别不良诱导和低质教材

每年假期，总有一些战友们（小朋友的父母们）给孩子提前预习下个学期的内容。

（各类培训机构、教育公众号等，都在**鼓励提前预习**——我个人持反对态度，回头单独展开说明我的考虑）

然而，很多培训机构、公众号，为了吸引流量，**诱导关注**后提供下载。

甚至有的战友们很辛苦的扫描、拍照等方式制作教材电子版，质量实在是难以描述。

——其实都不必，**国家免费提供了！**

——而且真正的 PDF，**可复制文字、图片**，不是图片扫描版！

**——再次感谢国家教育部门、出版社和平台！**

有红头文件为证：

> 2020年2月14日教育部《[关于发布中小学国家课程教材电子版链接的通告](https://www.gov.cn/xinwen/2020-02/14/content_5478551.htm)》

<img title="" src="./images_README/b060a97274d45a344ab679296094edbfd7ef87e1.png" alt="" width="1241">

# 附件和网盘下载

【最简版】<3M，可以直接在 Windows11 中运行，见附件

【独立版】约为9M，从网盘下载：

- 蓝奏云：[中小学教材下载器_极简版](https://wwk.lanzouw.com/b00tarda1c) 密码:hhns

- 123pan: https://www.123pan.com/s/W0s7Vv-JqVjh.html

- 百度盘: [百度网盘 请输入提取码](https://pan.baidu.com/s/1mdF3ql1_lEOexFWSXGyiFw?pwd=qrhh)

——如果不想使用【独立版】，请参考前面的【下载说明】，安装微软的 .NET 8 Rumtime 运行时，27M左右，一劳永逸。