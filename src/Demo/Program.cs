using Demo;

string selection = args.Length == 0 ? "all" : args[0];
if (args.Length > 1 || selection is not ("all" or "1" or "2" or "3" or "4"))
    throw new ArgumentException("Usage: Demo [all|1|2|3|4]");
if (selection is "all" or "1") Activity1.Run();
if (selection is "all" or "2") Activity2.Run();
if (selection is "all" or "3") Activity3.Run();
if (selection is "all" or "4") Activity4.Run();
