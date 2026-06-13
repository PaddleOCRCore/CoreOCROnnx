namespace WinFormsApp
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            buttonInit = new Button();
            buttonRec = new Button();
            textBoxResult = new TextBox();
            buttonGetBase64 = new Button();
            groupBox1 = new GroupBox();
            buttonFreeEngine = new Button();
            buttonPostFile = new Button();
            textBoxApiAddress = new TextBox();
            label8 = new Label();
            label7 = new Label();
            comboBoxModel = new ComboBox();
            buttonDownModels = new Button();
            numericUpDowncpu_mem = new NumericUpDown();
            label6 = new Label();
            numericUpDownThread = new NumericUpDown();
            label5 = new Label();
            label4 = new Label();
            comboBoxJson = new ComboBox();
            numDowncpu_threads = new NumericUpDown();
            label3 = new Label();
            numDowngpu_id = new NumericUpDown();
            label2 = new Label();
            label1 = new Label();
            comboBoxuse_gpu = new ComboBox();
            pictureBoxImg = new PictureBox();
            groupBox2 = new GroupBox();
            tabControlMain = new TabControl();
            tabPageOcr = new TabPage();
            tabPageYolo = new TabPage();
            groupBoxYolo = new GroupBox();
            labelYoloModel = new Label();
            comboBoxYoloModelType = new ComboBox();
            textBoxYoloModel = new TextBox();
            buttonBrowseYolo = new Button();
            labelYoloType = new Label();
            labelYoloGpu = new Label();
            comboBoxYoloUseGpu = new ComboBox();
            labelYoloGpuId = new Label();
            numericUpDownYoloGpuId = new NumericUpDown();
            labelYoloThreads = new Label();
            numericUpDownYoloThreads = new NumericUpDown();
            labelYoloConf = new Label();
            numericUpDownYoloConfidence = new NumericUpDown();
            labelYoloIou = new Label();
            numericUpDownYoloIou = new NumericUpDown();
            checkBoxYoloVisualize = new CheckBox();
            checkBoxYoloLog = new CheckBox();
            buttonYoloInit = new Button();
            buttonYoloDetectTensor = new Button();
            buttonYoloDetect = new Button();
            buttonYoloFree = new Button();
            groupBoxYoloImage = new GroupBox();
            pictureBoxYolo = new PictureBox();
            textBoxYoloResult = new TextBox();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDowncpu_mem).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownThread).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numDowncpu_threads).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numDowngpu_id).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxImg).BeginInit();
            groupBox2.SuspendLayout();
            tabControlMain.SuspendLayout();
            tabPageOcr.SuspendLayout();
            tabPageYolo.SuspendLayout();
            groupBoxYolo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDownYoloGpuId).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownYoloThreads).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownYoloConfidence).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownYoloIou).BeginInit();
            groupBoxYoloImage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxYolo).BeginInit();
            SuspendLayout();
            // 
            // buttonInit
            // 
            buttonInit.Location = new Point(606, 23);
            buttonInit.Name = "buttonInit";
            buttonInit.Size = new Size(87, 29);
            buttonInit.TabIndex = 0;
            buttonInit.Text = "初始化OCR";
            buttonInit.UseVisualStyleBackColor = true;
            buttonInit.Click += buttonInit_Click;
            // 
            // buttonRec
            // 
            buttonRec.Enabled = false;
            buttonRec.Location = new Point(699, 22);
            buttonRec.Name = "buttonRec";
            buttonRec.Size = new Size(214, 60);
            buttonRec.TabIndex = 1;
            buttonRec.Text = "OCR文本识别";
            buttonRec.UseVisualStyleBackColor = true;
            buttonRec.Click += buttonRec_Click;
            // 
            // textBoxResult
            // 
            textBoxResult.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            textBoxResult.Location = new Point(534, 143);
            textBoxResult.Multiline = true;
            textBoxResult.Name = "textBoxResult";
            textBoxResult.ScrollBars = ScrollBars.Both;
            textBoxResult.Size = new Size(547, 537);
            textBoxResult.TabIndex = 2;
            // 
            // buttonGetBase64
            // 
            buttonGetBase64.Location = new Point(919, 22);
            buttonGetBase64.Name = "buttonGetBase64";
            buttonGetBase64.Size = new Size(120, 28);
            buttonGetBase64.TabIndex = 3;
            buttonGetBase64.Text = "获取图片Base64";
            buttonGetBase64.UseVisualStyleBackColor = true;
            buttonGetBase64.Click += buttonGetBase64_Click;
            // 
            // groupBox1
            // 
            groupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox1.Controls.Add(buttonFreeEngine);
            groupBox1.Controls.Add(buttonPostFile);
            groupBox1.Controls.Add(textBoxApiAddress);
            groupBox1.Controls.Add(label8);
            groupBox1.Controls.Add(label7);
            groupBox1.Controls.Add(comboBoxModel);
            groupBox1.Controls.Add(buttonDownModels);
            groupBox1.Controls.Add(numericUpDowncpu_mem);
            groupBox1.Controls.Add(label6);
            groupBox1.Controls.Add(numericUpDownThread);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(comboBoxJson);
            groupBox1.Controls.Add(numDowncpu_threads);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(numDowngpu_id);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(comboBoxuse_gpu);
            groupBox1.Controls.Add(buttonGetBase64);
            groupBox1.Controls.Add(buttonInit);
            groupBox1.Controls.Add(buttonRec);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(1069, 125);
            groupBox1.TabIndex = 4;
            groupBox1.TabStop = false;
            groupBox1.Text = "功能选项";
            // 
            // buttonFreeEngine
            // 
            buttonFreeEngine.Enabled = false;
            buttonFreeEngine.Location = new Point(606, 54);
            buttonFreeEngine.Name = "buttonFreeEngine";
            buttonFreeEngine.Size = new Size(87, 29);
            buttonFreeEngine.TabIndex = 23;
            buttonFreeEngine.Text = "释放OCR";
            buttonFreeEngine.UseVisualStyleBackColor = true;
            buttonFreeEngine.Click += buttonFreeEngine_Click;
            // 
            // buttonPostFile
            // 
            buttonPostFile.Location = new Point(874, 87);
            buttonPostFile.Name = "buttonPostFile";
            buttonPostFile.Size = new Size(165, 28);
            buttonPostFile.TabIndex = 22;
            buttonPostFile.Text = "API接口测试";
            buttonPostFile.UseVisualStyleBackColor = true;
            buttonPostFile.Click += buttonPostFile_Click;
            // 
            // textBoxApiAddress
            // 
            textBoxApiAddress.Location = new Point(545, 89);
            textBoxApiAddress.Name = "textBoxApiAddress";
            textBoxApiAddress.Size = new Size(323, 23);
            textBoxApiAddress.TabIndex = 21;
            textBoxApiAddress.Text = "http://localhost:5000/OCRService/GetOCRFile";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(419, 92);
            label8.Name = "label8";
            label8.Size = new Size(114, 17);
            label8.TabIndex = 20;
            label8.Text = "WebApi接口地址：";
            label8.TextAlign = ContentAlignment.TopRight;
            label8.UseWaitCursor = true;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(18, 92);
            label7.Name = "label7";
            label7.Size = new Size(68, 17);
            label7.TabIndex = 19;
            label7.Text = "模型方案：";
            // 
            // comboBoxModel
            // 
            comboBoxModel.FormattingEnabled = true;
            comboBoxModel.Items.AddRange(new object[] { "PP-OCRv6_tiny", "PP-OCRv6_small", "PP-OCRv5_mobile", "PP-OCRv5_server", "PP-OCRv4_mobile", "PP-OCRv4_server" });
            comboBoxModel.Location = new Point(92, 89);
            comboBoxModel.Name = "comboBoxModel";
            comboBoxModel.Size = new Size(321, 25);
            comboBoxModel.TabIndex = 18;
            comboBoxModel.SelectedIndexChanged += comboBoxModel_SelectedIndexChanged;
            // 
            // buttonDownModels
            // 
            buttonDownModels.Location = new Point(919, 54);
            buttonDownModels.Name = "buttonDownModels";
            buttonDownModels.Size = new Size(120, 28);
            buttonDownModels.TabIndex = 16;
            buttonDownModels.Text = "下载OCR模型";
            buttonDownModels.UseVisualStyleBackColor = true;
            buttonDownModels.Click += buttonDownModels_Click;
            // 
            // numericUpDowncpu_mem
            // 
            numericUpDowncpu_mem.Location = new Point(545, 60);
            numericUpDowncpu_mem.Maximum = new decimal(new int[] { 8000, 0, 0, 0 });
            numericUpDowncpu_mem.Name = "numericUpDowncpu_mem";
            numericUpDowncpu_mem.Size = new Size(55, 23);
            numericUpDowncpu_mem.TabIndex = 15;
            numericUpDowncpu_mem.ValueChanged += numericUpDowncpu_mem_ValueChanged;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(419, 62);
            label6.Name = "label6";
            label6.Size = new Size(120, 17);
            label6.TabIndex = 14;
            label6.Text = "内存占用上限(MB)：";
            label6.TextAlign = ContentAlignment.TopRight;
            label6.UseWaitCursor = true;
            // 
            // numericUpDownThread
            // 
            numericUpDownThread.Location = new Point(333, 59);
            numericUpDownThread.Name = "numericUpDownThread";
            numericUpDownThread.Size = new Size(80, 23);
            numericUpDownThread.TabIndex = 13;
            numericUpDownThread.Value = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDownThread.ValueChanged += numericUpDownThread_ValueChanged;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(235, 61);
            label5.Name = "label5";
            label5.Size = new Size(92, 17);
            label5.TabIndex = 12;
            label5.Text = "模拟循环识别：";
            label5.TextAlign = ContentAlignment.TopRight;
            label5.UseWaitCursor = true;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(10, 62);
            label4.Name = "label4";
            label4.Size = new Size(76, 17);
            label4.TabIndex = 11;
            label4.Text = "输出JSON：";
            // 
            // comboBoxJson
            // 
            comboBoxJson.FormattingEnabled = true;
            comboBoxJson.Items.AddRange(new object[] { "只输出文字", "输出文字+JSON" });
            comboBoxJson.Location = new Point(92, 58);
            comboBoxJson.Name = "comboBoxJson";
            comboBoxJson.Size = new Size(129, 25);
            comboBoxJson.TabIndex = 10;
            comboBoxJson.SelectedIndexChanged += comboBoxJson_SelectedIndexChanged;
            // 
            // numDowncpu_threads
            // 
            numDowncpu_threads.Location = new Point(545, 27);
            numDowncpu_threads.Name = "numDowncpu_threads";
            numDowncpu_threads.Size = new Size(55, 23);
            numDowncpu_threads.TabIndex = 9;
            numDowncpu_threads.Value = new decimal(new int[] { 30, 0, 0, 0 });
            numDowncpu_threads.ValueChanged += numDowncpu_threads_ValueChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(459, 33);
            label3.Name = "label3";
            label3.Size = new Size(80, 17);
            label3.TabIndex = 8;
            label3.Text = "CPU线程数：";
            label3.TextAlign = ContentAlignment.TopRight;
            label3.UseWaitCursor = true;
            // 
            // numDowngpu_id
            // 
            numDowngpu_id.Location = new Point(333, 28);
            numDowngpu_id.Name = "numDowngpu_id";
            numDowngpu_id.Size = new Size(80, 23);
            numDowngpu_id.TabIndex = 7;
            numDowngpu_id.ValueChanged += numDowngpu_id_ValueChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(264, 31);
            label2.Name = "label2";
            label2.Size = new Size(63, 17);
            label2.TabIndex = 6;
            label2.Text = "GPU_ID：";
            label2.TextAlign = ContentAlignment.TopRight;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(17, 31);
            label1.Name = "label1";
            label1.Size = new Size(69, 17);
            label1.TabIndex = 5;
            label1.Text = "启用GPU：";
            // 
            // comboBoxuse_gpu
            // 
            comboBoxuse_gpu.FormattingEnabled = true;
            comboBoxuse_gpu.Items.AddRange(new object[] { "使用CPU", "使用GPU" });
            comboBoxuse_gpu.Location = new Point(92, 27);
            comboBoxuse_gpu.Name = "comboBoxuse_gpu";
            comboBoxuse_gpu.Size = new Size(129, 25);
            comboBoxuse_gpu.TabIndex = 4;
            comboBoxuse_gpu.SelectedIndexChanged += comboBoxuse_gpu_SelectedIndexChanged;
            // 
            // pictureBoxImg
            // 
            pictureBoxImg.Dock = DockStyle.Fill;
            pictureBoxImg.Location = new Point(3, 19);
            pictureBoxImg.Name = "pictureBoxImg";
            pictureBoxImg.Size = new Size(510, 515);
            pictureBoxImg.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxImg.TabIndex = 5;
            pictureBoxImg.TabStop = false;
            // 
            // groupBox2
            // 
            groupBox2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            groupBox2.Controls.Add(pictureBoxImg);
            groupBox2.Location = new Point(12, 143);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(516, 537);
            groupBox2.TabIndex = 6;
            groupBox2.TabStop = false;
            groupBox2.Text = "图片";
            // 
            // tabControlMain
            // 
            tabControlMain.Controls.Add(tabPageOcr);
            tabControlMain.Controls.Add(tabPageYolo);
            tabControlMain.Dock = DockStyle.Fill;
            tabControlMain.Location = new Point(0, 0);
            tabControlMain.Name = "tabControlMain";
            tabControlMain.SelectedIndex = 0;
            tabControlMain.Size = new Size(1093, 683);
            tabControlMain.TabIndex = 7;
            // 
            // tabPageOcr
            // 
            tabPageOcr.Controls.Add(groupBox2);
            tabPageOcr.Controls.Add(groupBox1);
            tabPageOcr.Controls.Add(textBoxResult);
            tabPageOcr.Location = new Point(4, 26);
            tabPageOcr.Name = "tabPageOcr";
            tabPageOcr.Padding = new Padding(3);
            tabPageOcr.Size = new Size(1085, 653);
            tabPageOcr.TabIndex = 0;
            tabPageOcr.Text = "OCR";
            tabPageOcr.UseVisualStyleBackColor = true;
            // 
            // tabPageYolo
            // 
            tabPageYolo.Controls.Add(groupBoxYolo);
            tabPageYolo.Controls.Add(groupBoxYoloImage);
            tabPageYolo.Controls.Add(textBoxYoloResult);
            tabPageYolo.Location = new Point(4, 26);
            tabPageYolo.Name = "tabPageYolo";
            tabPageYolo.Padding = new Padding(3);
            tabPageYolo.Size = new Size(1085, 653);
            tabPageYolo.TabIndex = 1;
            tabPageYolo.Text = "YOLO";
            tabPageYolo.UseVisualStyleBackColor = true;
            // 
            // groupBoxYolo
            // 
            groupBoxYolo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBoxYolo.Controls.Add(labelYoloModel);
            groupBoxYolo.Controls.Add(comboBoxYoloModelType);
            groupBoxYolo.Controls.Add(textBoxYoloModel);
            groupBoxYolo.Controls.Add(buttonBrowseYolo);
            groupBoxYolo.Controls.Add(labelYoloType);
            groupBoxYolo.Controls.Add(labelYoloGpu);
            groupBoxYolo.Controls.Add(comboBoxYoloUseGpu);
            groupBoxYolo.Controls.Add(labelYoloGpuId);
            groupBoxYolo.Controls.Add(numericUpDownYoloGpuId);
            groupBoxYolo.Controls.Add(labelYoloThreads);
            groupBoxYolo.Controls.Add(numericUpDownYoloThreads);
            groupBoxYolo.Controls.Add(labelYoloConf);
            groupBoxYolo.Controls.Add(numericUpDownYoloConfidence);
            groupBoxYolo.Controls.Add(labelYoloIou);
            groupBoxYolo.Controls.Add(numericUpDownYoloIou);
            groupBoxYolo.Controls.Add(checkBoxYoloVisualize);
            groupBoxYolo.Controls.Add(checkBoxYoloLog);
            groupBoxYolo.Controls.Add(buttonYoloInit);
            groupBoxYolo.Controls.Add(buttonYoloDetectTensor);
            groupBoxYolo.Controls.Add(buttonYoloDetect);
            groupBoxYolo.Controls.Add(buttonYoloFree);
            groupBoxYolo.Location = new Point(12, 12);
            groupBoxYolo.Name = "groupBoxYolo";
            groupBoxYolo.Size = new Size(1069, 125);
            groupBoxYolo.TabIndex = 0;
            groupBoxYolo.TabStop = false;
            groupBoxYolo.Text = "YOLO功能选项";
            // 
            // labelYoloModel
            // 
            labelYoloModel.AutoSize = true;
            labelYoloModel.Location = new Point(16, 30);
            labelYoloModel.Name = "labelYoloModel";
            labelYoloModel.Size = new Size(68, 17);
            labelYoloModel.TabIndex = 0;
            labelYoloModel.Text = "模型文件：";
            // 
            // comboBoxYoloModelType
            // 
            comboBoxYoloModelType.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxYoloModelType.Items.AddRange(new object[] { "Detect", "Pose", "Cls", "Seg", "OBB" });
            comboBoxYoloModelType.Location = new Point(92, 87);
            comboBoxYoloModelType.Name = "comboBoxYoloModelType";
            comboBoxYoloModelType.Size = new Size(100, 25);
            comboBoxYoloModelType.TabIndex = 4;
            // 
            // textBoxYoloModel
            // 
            textBoxYoloModel.Location = new Point(92, 26);
            textBoxYoloModel.Name = "textBoxYoloModel";
            textBoxYoloModel.ReadOnly = true;
            textBoxYoloModel.Size = new Size(363, 23);
            textBoxYoloModel.TabIndex = 1;
            // 
            // buttonBrowseYolo
            // 
            buttonBrowseYolo.Location = new Point(461, 26);
            buttonBrowseYolo.Name = "buttonBrowseYolo";
            buttonBrowseYolo.Size = new Size(80, 25);
            buttonBrowseYolo.TabIndex = 2;
            buttonBrowseYolo.Text = "选择";
            buttonBrowseYolo.UseVisualStyleBackColor = true;
            buttonBrowseYolo.Click += buttonBrowseYolo_Click;
            // 
            // labelYoloType
            // 
            labelYoloType.AutoSize = true;
            labelYoloType.Location = new Point(16, 91);
            labelYoloType.Name = "labelYoloType";
            labelYoloType.Size = new Size(68, 17);
            labelYoloType.TabIndex = 3;
            labelYoloType.Text = "模型类型：";
            // 
            // labelYoloGpu
            // 
            labelYoloGpu.AutoSize = true;
            labelYoloGpu.Location = new Point(18, 59);
            labelYoloGpu.Name = "labelYoloGpu";
            labelYoloGpu.Size = new Size(68, 17);
            labelYoloGpu.TabIndex = 5;
            labelYoloGpu.Text = "推理设备：";
            // 
            // comboBoxYoloUseGpu
            // 
            comboBoxYoloUseGpu.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxYoloUseGpu.Items.AddRange(new object[] { "CPU", "GPU" });
            comboBoxYoloUseGpu.Location = new Point(92, 56);
            comboBoxYoloUseGpu.Name = "comboBoxYoloUseGpu";
            comboBoxYoloUseGpu.Size = new Size(100, 25);
            comboBoxYoloUseGpu.TabIndex = 6;
            // 
            // labelYoloGpuId
            // 
            labelYoloGpuId.AutoSize = true;
            labelYoloGpuId.Location = new Point(200, 60);
            labelYoloGpuId.Name = "labelYoloGpuId";
            labelYoloGpuId.Size = new Size(63, 17);
            labelYoloGpuId.TabIndex = 7;
            labelYoloGpuId.Text = "GPU_ID：";
            // 
            // numericUpDownYoloGpuId
            // 
            numericUpDownYoloGpuId.Location = new Point(269, 57);
            numericUpDownYoloGpuId.Maximum = new decimal(new int[] { 16, 0, 0, 0 });
            numericUpDownYoloGpuId.Name = "numericUpDownYoloGpuId";
            numericUpDownYoloGpuId.Size = new Size(53, 23);
            numericUpDownYoloGpuId.TabIndex = 8;
            // 
            // labelYoloThreads
            // 
            labelYoloThreads.AutoSize = true;
            labelYoloThreads.Location = new Point(349, 60);
            labelYoloThreads.Name = "labelYoloThreads";
            labelYoloThreads.Size = new Size(44, 17);
            labelYoloThreads.TabIndex = 9;
            labelYoloThreads.Text = "线程：";
            // 
            // numericUpDownYoloThreads
            // 
            numericUpDownYoloThreads.Location = new Point(395, 57);
            numericUpDownYoloThreads.Maximum = new decimal(new int[] { 128, 0, 0, 0 });
            numericUpDownYoloThreads.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDownYoloThreads.Name = "numericUpDownYoloThreads";
            numericUpDownYoloThreads.Size = new Size(60, 23);
            numericUpDownYoloThreads.TabIndex = 10;
            numericUpDownYoloThreads.Value = new decimal(new int[] { 30, 0, 0, 0 });
            // 
            // labelYoloConf
            // 
            labelYoloConf.AutoSize = true;
            labelYoloConf.Location = new Point(337, 90);
            labelYoloConf.Name = "labelYoloConf";
            labelYoloConf.Size = new Size(56, 17);
            labelYoloConf.TabIndex = 11;
            labelYoloConf.Text = "置信度：";
            // 
            // numericUpDownYoloConfidence
            // 
            numericUpDownYoloConfidence.DecimalPlaces = 2;
            numericUpDownYoloConfidence.Increment = new decimal(new int[] { 5, 0, 0, 131072 });
            numericUpDownYoloConfidence.Location = new Point(395, 88);
            numericUpDownYoloConfidence.Maximum = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDownYoloConfidence.Minimum = new decimal(new int[] { 1, 0, 0, 131072 });
            numericUpDownYoloConfidence.Name = "numericUpDownYoloConfidence";
            numericUpDownYoloConfidence.Size = new Size(60, 23);
            numericUpDownYoloConfidence.TabIndex = 12;
            numericUpDownYoloConfidence.Value = new decimal(new int[] { 25, 0, 0, 131072 });
            // 
            // labelYoloIou
            // 
            labelYoloIou.AutoSize = true;
            labelYoloIou.Location = new Point(220, 92);
            labelYoloIou.Name = "labelYoloIou";
            labelYoloIou.Size = new Size(43, 17);
            labelYoloIou.TabIndex = 13;
            labelYoloIou.Text = "IOU：";
            // 
            // numericUpDownYoloIou
            // 
            numericUpDownYoloIou.DecimalPlaces = 2;
            numericUpDownYoloIou.Increment = new decimal(new int[] { 5, 0, 0, 131072 });
            numericUpDownYoloIou.Location = new Point(269, 88);
            numericUpDownYoloIou.Maximum = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDownYoloIou.Minimum = new decimal(new int[] { 1, 0, 0, 131072 });
            numericUpDownYoloIou.Name = "numericUpDownYoloIou";
            numericUpDownYoloIou.Size = new Size(53, 23);
            numericUpDownYoloIou.TabIndex = 14;
            numericUpDownYoloIou.Value = new decimal(new int[] { 45, 0, 0, 131072 });
            // 
            // checkBoxYoloVisualize
            // 
            checkBoxYoloVisualize.AutoSize = true;
            checkBoxYoloVisualize.Checked = true;
            checkBoxYoloVisualize.CheckState = CheckState.Checked;
            checkBoxYoloVisualize.Location = new Point(461, 59);
            checkBoxYoloVisualize.Name = "checkBoxYoloVisualize";
            checkBoxYoloVisualize.Size = new Size(87, 21);
            checkBoxYoloVisualize.TabIndex = 15;
            checkBoxYoloVisualize.Text = "可视化输出";
            checkBoxYoloVisualize.UseVisualStyleBackColor = true;
            // 
            // checkBoxYoloLog
            // 
            checkBoxYoloLog.AutoSize = true;
            checkBoxYoloLog.Location = new Point(461, 89);
            checkBoxYoloLog.Name = "checkBoxYoloLog";
            checkBoxYoloLog.Size = new Size(75, 21);
            checkBoxYoloLog.TabIndex = 16;
            checkBoxYoloLog.Text = "启用日志";
            checkBoxYoloLog.UseVisualStyleBackColor = true;
            // 
            // buttonYoloInit
            // 
            buttonYoloInit.Location = new Point(558, 26);
            buttonYoloInit.Name = "buttonYoloInit";
            buttonYoloInit.Size = new Size(100, 35);
            buttonYoloInit.TabIndex = 17;
            buttonYoloInit.Text = "初始化YOLO";
            buttonYoloInit.UseVisualStyleBackColor = true;
            buttonYoloInit.Click += buttonYoloInit_Click;
            // 
            // buttonYoloDetectTensor
            // 
            buttonYoloDetectTensor.Enabled = false;
            buttonYoloDetectTensor.Location = new Point(792, 26);
            buttonYoloDetectTensor.Name = "buttonYoloDetectTensor";
            buttonYoloDetectTensor.Size = new Size(122, 81);
            buttonYoloDetectTensor.TabIndex = 18;
            buttonYoloDetectTensor.Text = "YOLO识别Tensor";
            buttonYoloDetectTensor.UseVisualStyleBackColor = true;
            buttonYoloDetectTensor.Click += buttonYoloDetectTensor_Click;
            // 
            // buttonYoloDetect
            // 
            buttonYoloDetect.Enabled = false;
            buttonYoloDetect.Location = new Point(664, 26);
            buttonYoloDetect.Name = "buttonYoloDetect";
            buttonYoloDetect.Size = new Size(122, 81);
            buttonYoloDetect.TabIndex = 18;
            buttonYoloDetect.Text = "YOLO识别Json";
            buttonYoloDetect.UseVisualStyleBackColor = true;
            buttonYoloDetect.Click += buttonYoloDetect_Click;
            // 
            // buttonYoloFree
            // 
            buttonYoloFree.Enabled = false;
            buttonYoloFree.Location = new Point(558, 72);
            buttonYoloFree.Name = "buttonYoloFree";
            buttonYoloFree.Size = new Size(100, 35);
            buttonYoloFree.TabIndex = 19;
            buttonYoloFree.Text = "释放YOLO";
            buttonYoloFree.UseVisualStyleBackColor = true;
            buttonYoloFree.Click += buttonYoloFree_Click;
            // 
            // groupBoxYoloImage
            // 
            groupBoxYoloImage.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            groupBoxYoloImage.Controls.Add(pictureBoxYolo);
            groupBoxYoloImage.Location = new Point(12, 143);
            groupBoxYoloImage.Name = "groupBoxYoloImage";
            groupBoxYoloImage.Size = new Size(516, 507);
            groupBoxYoloImage.TabIndex = 1;
            groupBoxYoloImage.TabStop = false;
            groupBoxYoloImage.Text = "YOLO图片";
            // 
            // pictureBoxYolo
            // 
            pictureBoxYolo.Dock = DockStyle.Fill;
            pictureBoxYolo.Location = new Point(3, 19);
            pictureBoxYolo.Name = "pictureBoxYolo";
            pictureBoxYolo.Size = new Size(510, 485);
            pictureBoxYolo.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxYolo.TabIndex = 0;
            pictureBoxYolo.TabStop = false;
            // 
            // textBoxYoloResult
            // 
            textBoxYoloResult.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            textBoxYoloResult.Location = new Point(534, 143);
            textBoxYoloResult.Multiline = true;
            textBoxYoloResult.Name = "textBoxYoloResult";
            textBoxYoloResult.ScrollBars = ScrollBars.Both;
            textBoxYoloResult.Size = new Size(547, 507);
            textBoxYoloResult.TabIndex = 2;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1093, 683);
            Controls.Add(tabControlMain);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "CoreOCROpenVino识别Demo V4.0.0--QQ群：475159576 https://github.com/PaddleOCRCore/CoreOCROnnx.git";
            Load += MainForm_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDowncpu_mem).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownThread).EndInit();
            ((System.ComponentModel.ISupportInitialize)numDowncpu_threads).EndInit();
            ((System.ComponentModel.ISupportInitialize)numDowngpu_id).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxImg).EndInit();
            groupBox2.ResumeLayout(false);
            tabControlMain.ResumeLayout(false);
            tabPageOcr.ResumeLayout(false);
            tabPageOcr.PerformLayout();
            tabPageYolo.ResumeLayout(false);
            tabPageYolo.PerformLayout();
            groupBoxYolo.ResumeLayout(false);
            groupBoxYolo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDownYoloGpuId).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownYoloThreads).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownYoloConfidence).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownYoloIou).EndInit();
            groupBoxYoloImage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBoxYolo).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Button buttonInit;
        private Button buttonRec;
        private TextBox textBoxResult;
        private Button buttonGetBase64;
        private GroupBox groupBox1;
        private Label label1;
        private ComboBox comboBoxuse_gpu;
        private Label label2;
        private NumericUpDown numDowngpu_id;
        private NumericUpDown numDowncpu_threads;
        private Label label3;
        private PictureBox pictureBoxImg;
        private GroupBox groupBox2;
        private Label label4;
        private ComboBox comboBoxJson;
        private NumericUpDown numericUpDownThread;
        private Label label5;
        private NumericUpDown numericUpDowncpu_mem;
        private Label label6;
        private Button buttonDownModels;
        private Label label7;
        private ComboBox comboBoxModel;
        private Label label8;
        private TextBox textBoxApiAddress;
        private Button buttonPostFile;
        private Button buttonFreeEngine;
        private TabControl tabControlMain;
        private TabPage tabPageOcr;
        private TabPage tabPageYolo;
        private GroupBox groupBoxYolo;
        private Label labelYoloModel;
        private TextBox textBoxYoloModel;
        private Button buttonBrowseYolo;
        private Label labelYoloType;
        private ComboBox comboBoxYoloModelType;
        private Label labelYoloGpu;
        private ComboBox comboBoxYoloUseGpu;
        private Label labelYoloGpuId;
        private NumericUpDown numericUpDownYoloGpuId;
        private Label labelYoloThreads;
        private NumericUpDown numericUpDownYoloThreads;
        private Label labelYoloConf;
        private NumericUpDown numericUpDownYoloConfidence;
        private Label labelYoloIou;
        private NumericUpDown numericUpDownYoloIou;
        private CheckBox checkBoxYoloVisualize;
        private CheckBox checkBoxYoloLog;
        private Button buttonYoloInit;
        private Button buttonYoloDetect;
        private Button buttonYoloFree;
        private GroupBox groupBoxYoloImage;
        private PictureBox pictureBoxYolo;
        private TextBox textBoxYoloResult;
        private Button buttonYoloDetectTensor;
    }
}
