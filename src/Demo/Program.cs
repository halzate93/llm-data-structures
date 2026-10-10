using Demo;

string selection = args.Length == 0 ? "all" : args[0];
if (args.Length > 1 || selection is not ("all" or "1"))
    throw new ArgumentException("Usage: Demo [all|1]");
if (selection is "all" or "1") Activity1.Run();
