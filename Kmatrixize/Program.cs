// Main
// read arguments
using Kmatrixize;

if (args.Length < 2)
{
    help();
    return;
}

// arg0 => input file
// arg1 => output file
// arg2 => lowest count number (optinal, default 0, only for PointMax and EdgeMax)
// arg3 => method (optinal, default PointMax)
string input_file = args[0];
string output_file = args[1];

int lowest_n = 0;
if (args.Length >= 3)
{
    lowest_n = Convert.ToInt32(args[2]);
}

Method m = Method.PointMax;
if (args.Length == 4)
{
    m = (Method) Convert.ToInt32(args[3]);
}


// check input file
if (!File.Exists(input_file))
{
    Console.Error.WriteLine("Input file not exsits!");
    return;
}

// check output file directory
var f = Path.GetDirectoryName(output_file);
if (f != "" && !Directory.Exists(f))
{
    Console.WriteLine($"Output path not exsits! Creating output folder: [{f}]");
    Directory.CreateDirectory(f);
}

// print argument info
Console.WriteLine($"Converting populational hic data: {input_file} to K-matrix {output_file}");
Console.WriteLine("\tUsing Method: " + m);

// record starting point
var start = DateTime.Now;

// run method
switch (m)
{
    case Method.PointMax:
        Methods.PointMax(input_file, output_file, lowest_n);
        break;
    case Method.EdgeMax:
        Methods.EdgeMax(input_file, output_file, lowest_n);
        break;
    case Method.Random:
        Methods.Random(input_file, output_file);
        break;
    default:
        break;
}

// end of transformation
var end = DateTime.Now;
Console.WriteLine("Completed transformation. Total time consumption: " + (end - start).TotalSeconds + " seconds!");

// help messages
void help()
{
    Console.WriteLine("\tUsage: Kmatrixize [1.Input file] [2.output file] [3.threshold] [4.method]");
    Console.WriteLine("\tMethod: 0 -> PointMax [default], 1 -> EdgeMax, 2 -> Random");
    Console.WriteLine("\tThreshold: [default -> 0]; for PointMax -> lowest number of contact (edge) number for each point (segments); for EdgeMax -> lowest number of contact (edge) number for each point (segments) pair;");
    Console.WriteLine("\tExample: Kmatrixize A8.ncc A8.P0.ncc 2 0");
}
