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
                new test_xray自动标注()
            );

            testManager.AddTests(
                "tools",
                new test_删除XRay_C2文件夹内的无效链接(),
                new test_将yolo标签全改为0(),
                new test_简单图像搜索(),
                new test_将目录下的图像全部改为指定格式(),
                new test_生成目标检测和语义分割数据集()
            );


            testManager.ShowTestEntrance();
        }
    }
}
