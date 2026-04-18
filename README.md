## 新一代神秘模组

Recently, some people have accused my code of plagiarizing other people's closed-source code and claiming that I misused AI. This is pure rumor.

近期，有人指控我的代码抄袭了他人的闭源代码，并认为我不当使用了 AI。这是纯纯的谣言。

As proof, the AI usage report is at the bottom.

作为自证，AI 使用报告在底部。

In addition, I made a video to refute these rumors.

此外，我制作了视频来驳斥这些谣言。

[【【AIC多人联机】针对模组抄袭谣言的回应，以及满满的干货】](https://www.bilibili.com/video/BV1ZCd5ByEoh/?share_source=copy_web&vd_source=9e9a81c54a2e2739e919ab32cad87455)

---

https://github.com/e9ae9933/Kaleidoscopic

这是客户端模组。

如果你正在寻找服务器，请转到最底下。

---

## 开发环境配置

1. 准备一个纯净原版的 029j2 安装。（理论上029通用但没有尝试过，可能出现神秘问题）

1. 安装 BepInEx，从这里 https://github.com/BepInEx/BepInEx/releases 下载并安装。

特别注意本模组只支持 BepInEx 5 打头的模组加载器！

1. 把仓库 clone 到你的本地

1. 参照 Kaleidoscopic.csproj 开头，配置路径信息。

你可能或找到类似于这个东西：

```csharp
<!--
    Write these to Kaleidoscopic.csproj.user.
    
    <?xml version="1.0" encoding="utf-8"?>
    <Project>
        <PropertyGroup>
            <GameDir>path\to\AliceInCradle</GameDir>
        </PropertyGroup>
    </Project>
    
    Where $(GameDir)\AliceInCradle.exe is present.
-->
```
这段话的意思是，让你在项目目录新增一个叫做 `Kaleidoscopic.csproj.user` 的文件。

在文件里写下如下内容，其中游戏安装路径是直接含有 `AliceInCradle(.exe)` 的文件。

```csharp
<?xml version="1.0" encoding="utf-8"?>
<Project>
    <PropertyGroup>
        <GameDir>到你的游戏安装的路径</GameDir>
    </PropertyGroup>
</Project>
```

然后，进行构建，如果你可以在 `BepInEx\plugins\Kaleidoscopic` 文件夹下看到 `Kaleidosopic.dll` 文件，说明构建成功。

## 服务端-客户端联机原理

客户端告诉服务端自己所在的地图和诺艾儿的位置信息，然后服务端广播给同地图的其他客户端。

客户端同步后，进行绘画。

因此，任何人都可以开自己的私人或公开服务器。服务器有自动构建，地址为

https://github.com/e9ae9933/Kaleidoscopic-Server/actions

点击带有绿钩的构建，找到最下的 Artifacts，你可以看到 `Kaleidoscopic-Server-服务端版本号-SNAPSHOT-all.jar`。

## 如何开服

如果你开过别的游戏服务器对你来说可能会比较轻松

本服务器用法和我的世界很类似

在这里只提供 `Windows` 教程，如果你是 `Linux` 和 `MacOS` 用户相信你已经知道怎么做了

### 第一步：准备 Java 环境
首先，你需要 Java 环境。我们需要 Java 21 或以上的环境。

【如何检测 Java 环境】
1. 按下键盘上的 Win + R 键，输入 cmd 并按回车。
2. 在弹出的黑框中输入 java -version 并按回车。
3. 如果显示版本号为 21 或以上，说明准备就绪。如果提示找不到命令，请前往下方链接下载并安装：
   下载地址：https://mirrors.tuna.tsinghua.edu.cn/Adoptium/21/jdk/x64/windows/
   (安装时请一直点下一步，并确保勾选了“添加到 PATH 环境变量”相关的选项)

### 第二步：创建启动脚本并开服
我们需要创建一个 run.bat 文件来快捷启动服务器。

【如何生成 run.bat】
1. 在当前文件夹内，右键空白处 -> 新建 -> 文本文档。
2. 打开新建的文本文档，复制并粘贴以下两行代码：
3. -Xms1G 的意思是最小使用 1G 内存，-Xmx2G 的意思是最多使用 2G 内存。

```cmd
java -Xms1G -Xmx2G -jar Kaleidoscopic-Server-1.0-SNAPSHOT-all.jar
pause
```

3. 保存并关闭文件。
4. 将该文件重命名为 run.bat (如果弹窗提示修改扩展名可能导致文件不可用，点击“是”)。
5. 双击运行 run_server.bat，如果看到控制台开始跑代码，就说明服务器正在启动啦！

服务器会运行在本地端口 25560 上。如果你要联机的朋友不在你的本地，你可能需要端口映射。

服务端采用 `TCP` 的 `websocket`，请注意如果使用公用的端口映射，在大陆的一些地方可能会要求备案。推荐使用香港节点。

### 第三步：加入游戏

按F1（有的电脑是Fn+F1），输入服务器地址即可。你可能需要重新打开多人联机开关以应用更改。


---

## AI 使用报告

本项目的后端 Java 语言采用了 LLM 大语言模型 Gemini 辅助开发。

我们使用 AI 生成了服务端的代码，因为这玩意真没啥含金量。

我们的客户端 C# 部分**仅**使用 AI 写网络通讯部分，以及**不影响核心功能地**修改已有手写代码。

所有的 AI 代码均经过人工审查。

The backend Java code for this project was developed using the LLM (Large Language Model) Gemini.

We used AI to generate the server-side code because it's not particularly valuable.

Our client-side C# code **only** uses AI to write the network communication parts, and **modifies** existing hand-written code **without affecting core functionality**.

All AI-generated code has been manually reviewed.
