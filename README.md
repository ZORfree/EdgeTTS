# EdgeTTS
这是一个使用C#编写的，使用Edge的语音合成引擎的程序，仅供于学习交流。代码很简单，后续有时间上传，感谢各位大佬的开源代码和库，才能快速实现。

![image](https://github.com/ZORfree/EdgeTTS/assets/26847215/21a0eb6d-bd5f-4dad-93e8-e9f209cd5014)

目前实现功能如下：
## V0.1
- 输入文本框、试听|停止按钮、保存位置（默认当前目录）、语种选择框（可输入过滤）
- 添加语速变换、进度条展示功能
- 添加合成某个语种的所有发言人的一键合成功能
- 默认将音频转换为16k 16bit的wav音频格式
- 防止多次点击造成的重复合成逻辑
## V0.2.1
- 支持批量合成多项，中间用#分割即可，试听将过滤#号
- 为防止多发言人多词文件被覆盖，增加索引后缀，并在output文件夹下覆盖本次的合成音频的scp及其lab
- 标题版本跟随文件版本
