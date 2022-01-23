using Ookii.Dialogs.Wpf;
using OpenCvSharp;
using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace ArknightsSpriteMatting
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        private delegate void ProcessingDelegate(string texture2DDirectory, string[] spriteDumpFiles, string outputDirectory, bool isCrop, bool isBottomLeft);
        private delegate void LogDelegate(string s);

        public MainWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
        }

        private void Texture2DButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog();
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                this.Texture2DTextBox.Text = dialog.SelectedPath;
            }
        }

        private void SpriteDumpButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog();
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                this.SpriteDumpTextBox.Text = dialog.SelectedPath;
                this.OutputTextBox.Text = Path.Combine(dialog.SelectedPath, "output");
            }
        }

        private void OutputButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog();
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                this.OutputTextBox.Text = dialog.SelectedPath;
            }
        }

        private void CropCheckBox_Click(object sender, RoutedEventArgs e)
        {
            var isEnable = !(this.CropCheckBox.IsChecked ?? false);
            this.BottomLeftRadioButton.IsEnabled = isEnable;
            this.CenterRadioButton.IsEnabled = isEnable;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            // 获取组件值
            var texture2DDirectory = this.Texture2DTextBox.Text.Trim();
            var spriteDumpDirectory = this.SpriteDumpTextBox.Text.Trim();
            var outputDirectory = this.OutputTextBox.Text.Trim();
            var isCrop = this.CropCheckBox.IsChecked ?? false;
            var isBottomLeft = this.BottomLeftRadioButton.IsChecked ?? false;
            // 判断文件夹是否存在
            if (!Directory.Exists(texture2DDirectory))
            {
                MessageBox.Show("Texture2D 文件夹不存在", "导出失败", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!Directory.Exists(spriteDumpDirectory))
            {
                MessageBox.Show("Sprite 转储文件夹不存在", "导出失败", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            // 创建导出文件夹
            Directory.CreateDirectory(outputDirectory);
            // 禁用所有组件
            this.Texture2DTextBox.IsEnabled = false;
            this.Texture2DButton.IsEnabled = false;
            this.SpriteDumpTextBox.IsEnabled = false;
            this.SpriteDumpButton.IsEnabled = false;
            this.OutputTextBox.IsEnabled = false;
            this.OutputButton.IsEnabled = false;
            this.CropCheckBox.IsEnabled = false;
            this.BottomLeftRadioButton.IsEnabled = false;
            this.CenterRadioButton.IsEnabled = false;
            this.ConfirmButton.IsEnabled = false;
            this.CloseButton.IsEnabled = false;
            // 获取 Sprite 转储文件夹下所有 txt 文件
            var spriteDumpFiles = Directory.GetFiles(spriteDumpDirectory, "*.txt");
            // 初始化
            this.ProgressBar.Value = 0;
            this.ProgressBar.Maximum = spriteDumpFiles.Length;
            this.LogTextBox.Text = "";
            // 进行导出操作
            var processor = new ProcessingDelegate(Processing);
            processor.BeginInvoke(texture2DDirectory, spriteDumpFiles, outputDirectory, isCrop, isBottomLeft, (result) =>
            {
                // 解除禁用所有组件
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    this.Texture2DTextBox.IsEnabled = true;
                    this.Texture2DButton.IsEnabled = true;
                    this.SpriteDumpTextBox.IsEnabled = true;
                    this.SpriteDumpButton.IsEnabled = true;
                    this.OutputTextBox.IsEnabled = true;
                    this.OutputButton.IsEnabled = true;
                    this.CropCheckBox.IsEnabled = true;
                    var isEnable = !(this.CropCheckBox.IsChecked ?? false);
                    this.BottomLeftRadioButton.IsEnabled = isEnable;
                    this.CenterRadioButton.IsEnabled = isEnable;
                    this.ConfirmButton.IsEnabled = true;
                    this.CloseButton.IsEnabled = true;
                }));
            }, null);
        }

        private void Processing(string texture2DDirectory, string[] spriteDumpFiles, string outputDirectory, bool isCrop, bool isBottomLeft)
        {
            var cnt = 0;
            // 读取所有 txt 文件
            foreach (var file in spriteDumpFiles)
            {
                // 读取文件
                var lines = File.ReadAllLines(file);
                // 变量定义
                string textureName = "", alphaTextureName = "";
                int x = -1, y = -1, width = -1, height = -1, croppedWidth = -1, croppedHeight = -1;
                // 遍历文件每一行
                for (var i = 0; i < lines.Length; i++)
                {
                    // 为图片宽度和高度赋值
                    if (lines[i].Contains("Rectf m_Rect"))
                    {
                        for (var j = i + 1; j < i + 5; j++)
                        {
                            if (lines[j].Contains("width"))
                            {
                                width = (int) Math.Round(decimal.Parse(GetFromEntry(lines[j])), MidpointRounding.AwayFromZero);
                            }
                            if (lines[j].Contains("height"))
                            {
                                height = (int) Math.Round(decimal.Parse(GetFromEntry(lines[j])), MidpointRounding.AwayFromZero);
                            }
                        }
                    }
                    // 为图片原材质赋值
                    if (lines[i].Contains("PPtr<Texture2D> texture"))
                    {
                        for (var j = i + 1; j < i + 3; j++)
                        {
                            if (lines[j].Contains("m_PathID"))
                            {
                                textureName = Path.Combine(texture2DDirectory, Path.ChangeExtension(GetFromEntry(lines[j]), "png"));
                            }
                        }
                    }
                    // 为图片 alpha 通道材质赋值
                    if (lines[i].Contains("PPtr<Texture2D> alphaTexture"))
                    {
                        for (var j = i + 1; j < i + 3; j++)
                        {
                            if (lines[j].Contains("m_PathID"))
                            {
                                alphaTextureName = Path.Combine(texture2DDirectory, Path.ChangeExtension(GetFromEntry(lines[j]), "png"));
                            }
                        }
                    }
                    // 为图片 x 坐标位置、y 坐标位置、裁剪宽度和裁剪高度赋值
                    if (lines[i].Contains("Rectf textureRect"))
                    {
                        for (var j = i + 1; j < i + 5; j++)
                        {
                            if (lines[j].Contains("x"))
                            {
                                x = (int) Math.Round(decimal.Parse(GetFromEntry(lines[j])), MidpointRounding.AwayFromZero);
                            }
                            if (lines[j].Contains("y"))
                            {
                                y = (int) Math.Round(decimal.Parse(GetFromEntry(lines[j])), MidpointRounding.AwayFromZero);
                            }
                            if (lines[j].Contains("width"))
                            {
                                croppedWidth = (int) Math.Round(decimal.Parse(GetFromEntry(lines[j])), MidpointRounding.AwayFromZero);
                            }
                            if (lines[j].Contains("height"))
                            {
                                croppedHeight = (int) Math.Round(decimal.Parse(GetFromEntry(lines[j])), MidpointRounding.AwayFromZero);
                            }
                        }
                    }
                }
                // 检查定义情况
                if (width == -1 || height == -1) {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => this.ProgressBar.Value++));
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new LogDelegate(Log), $"处理 {Path.GetFileNameWithoutExtension(file)} 失败，图片的宽度或高度未定义，可能是转储文件的对应字段发生了改变");
                    continue;
                }
                if (string.IsNullOrEmpty(textureName) || string.IsNullOrEmpty(alphaTextureName))
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => this.ProgressBar.Value++));
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new LogDelegate(Log), $"处理 {Path.GetFileNameWithoutExtension(file)} 失败，图片的原材质或 alpha 通道材质未定义，可能是转储文件的对应字段发生了改变");
                    continue;
                }
                if (!File.Exists(textureName))
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => this.ProgressBar.Value++));
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new LogDelegate(Log), $"处理 {Path.GetFileNameWithoutExtension(file)} 失败，图片的原材质 {Path.GetFileName(textureName)} 不存在");
                    continue;
                }
                if (!File.Exists(alphaTextureName))
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => this.ProgressBar.Value++));
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new LogDelegate(Log), $"处理 {Path.GetFileNameWithoutExtension(file)} 失败，图片的 alpha 通道材质 {Path.GetFileName(alphaTextureName)} 不存在");
                    continue;
                }
                if (x == -1 || y == -1)
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => this.ProgressBar.Value++));
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new LogDelegate(Log), $"处理 {Path.GetFileNameWithoutExtension(file)} 失败，图片的 x 坐标位置或 y 坐标位置未定义，可能是转储文件的对应字段发生了改变");
                    continue;
                }
                if (croppedWidth == -1 || croppedHeight == -1)
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => this.ProgressBar.Value++));
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new LogDelegate(Log), $"处理 {Path.GetFileNameWithoutExtension(file)} 失败，图片的裁剪宽度或裁剪高度未定义，可能是转储文件的对应字段发生了改变");
                    continue;
                }
                // 读取原图
                var texture = Cv2.ImRead(textureName);
                // 以灰度图模式读取 alpha 通道图
                var alphaTexture = Cv2.ImRead(alphaTextureName, ImreadModes.Grayscale);
                // 将 alpha 通道图大小 resize 为原图大小
                alphaTexture.Resize(texture.Size());
                // 根据裁剪宽度和裁剪高度将原图和 alpha 通道图进行裁剪
                var croppedRect = new OpenCvSharp.Rect(x, texture.Height - y - croppedHeight, croppedWidth, croppedHeight);
                var croppedTexture = new Mat(texture, croppedRect);
                var croppedAlphaTexture = new Mat(alphaTexture, croppedRect);
                // 分离 bgr 通道
                var bgr = croppedTexture.Split();
                // 新建 bgra 通道，alpha 通道值为 alpha 通道图的灰度值
                var bgra = new Mat[4];
                // 开启裁剪空白区域则直接复制相应的值即可
                if (isCrop)
                {
                    for (var i = 0; i < 3; i++)
                    {
                        bgra[i] = bgr[i];
                    }
                    bgra[3] = croppedAlphaTexture;
                }
                else
                {
                    // 位于左下角
                    if (isBottomLeft)
                    {
                        // 创建粘贴区域 Rect
                        var rect = new OpenCvSharp.Rect(0, height - croppedHeight, croppedWidth, croppedHeight);
                        for (var i = 0; i < 3; i++)
                        {
                            // 创建空白通道
                            bgra[i] = Mat.Zeros(new OpenCvSharp.Size(width, height), bgr[i].Type());
                            // 创建粘贴区域 Mat
                            var matWithRect = new Mat(bgra[i], rect);
                            // 将原通道复制进粘贴区域 Mat
                            bgr[i].CopyTo(matWithRect);
                        }
                        // 创建空白通道
                        bgra[3] = Mat.Zeros(new OpenCvSharp.Size(width, height), croppedAlphaTexture.Type());
                        // 创建粘贴区域 Mat
                        var alphaMatWithRect = new Mat(bgra[3], rect);
                        // 将原通道复制进粘贴区域 Mat
                        croppedAlphaTexture.CopyTo(alphaMatWithRect);
                    }
                    // 居中
                    else
                    {
                        // 创建粘贴区域 Rect
                        var rect = new OpenCvSharp.Rect((width - croppedWidth) / 2, (height - croppedHeight) / 2, croppedWidth, croppedHeight);
                        for (var i = 0; i < 3; i++)
                        {
                            // 创建空白通道
                            bgra[i] = Mat.Zeros(new OpenCvSharp.Size(width, height), bgr[i].Type());
                            // 创建粘贴区域 Mat
                            var matWithRect = new Mat(bgra[i], rect);
                            // 将原通道复制进粘贴区域 Mat
                            bgr[i].CopyTo(matWithRect);
                        }
                        // 创建空白通道
                        bgra[3] = Mat.Zeros(new OpenCvSharp.Size(width, height), croppedAlphaTexture.Type());
                        // 创建粘贴区域 Mat
                        var alphaMatWithRect = new Mat(bgra[3], rect);
                        // 将原通道复制进粘贴区域 Mat
                        croppedAlphaTexture.CopyTo(alphaMatWithRect);
                    }
                }
                // 将 bgra 通道合并为图像
                var mattedTexture = new Mat();
                Cv2.Merge(bgra, mattedTexture);
                // 保存图像
                Cv2.ImWrite(Path.Combine(outputDirectory, Path.ChangeExtension(Path.GetFileName(file), "png")), mattedTexture);
                // 更新进度条与日志
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => this.ProgressBar.Value++));
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new LogDelegate(Log), $"处理 {Path.GetFileNameWithoutExtension(file)} 成功");
                cnt++;
            }
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new LogDelegate(Log), $"成功处理 {cnt} 个 Sprite 文件，共 {spriteDumpFiles.Length} 个");
        }

        private static string GetFromEntry(string s)
        {
            return s.Substring(s.IndexOf("=", StringComparison.Ordinal) + 1).Trim();
        }

        private void Log(string s)
        {
            this.LogTextBox.Text += s + "\n";
            this.LogTextBox.ScrollToEnd();
        }
    }
}
