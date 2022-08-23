Program.cs 是根程序，整个程序的常量都在这里定义，它只是程序入口，决定了模拟哪个id

Properties是程序脚本文件夹
    SlotsBaseClass.cs 是基础类，SlotsComputer中要用的一些基础类型都在这里面，比如说有symbol，reel，line等
    SlotsTools.cs 是单次spin或模拟需要用的一些工具方法 比如说从矩阵中获取特定元素的数量，把字典转化为字符串
    
    /DoSlots 是模拟逻辑文件夹，模拟过程在这里控制，输出进度，定义输出excel的格式，输出excel都在这一层； DoSlotsById是基类，有特殊规则统一继承它
    /SlotsComputer 是slotsSpin规则逻辑文件夹，单次spin的逻辑都在这里；SlotsComputer是基类，有特殊规则统一继承它
    /FileMethod 是文件操作规则逻辑文件夹
        DataTableMethod.cs  所有dataTable的处理方法都在这里
        ExcelMethod.cs 所有Excel的处理方法都在这里
        LogFile.cs log文件的处理方法在这里，输出日志统一用里面的saveLog方便管理
        

        