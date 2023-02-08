using AIDatasetsPro.src;
using work.test;
using work.test.Interfaces;

namespace AIDatasetsPro
{
    class Program
    {
        static void Main(string[] args)
        {
            ITestManager testManager = new ConsoleTestManager();

            testManager.AddTests(
                "自动标注",
                new test_XRay基于模板匹配的自动标注()
            );
            testManager.AddTests(
                "XRray卷盘",
                new test_XRay卷盘_调整图像尺寸_四周补零(),
                new test_XRay卷盘_修改XML值(),
                new test_XRay卷盘_XML转DOTA()
            );
            testManager.AddTests(
                "tools",
                new test_删除XRay_C2文件夹内的无效链接(),
                new test_将yolo标签全改为0(),
                new test_简单图像搜索(),
                new test_将目录下的图像全部改为指定格式(),
                new test_生成目标检测和语义分割数据集(),
                new test_获取目录下的所有文件列表(),
                new test_将目标文件夹的图像按比例缩放至1024并补零()
            );

            testManager.ShowTestEntrance();
        }
    }
}
