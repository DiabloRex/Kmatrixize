# Kmatrixize
Covert populational hic ncc format (nuc_process) matrix to Genome Khimaira matrix

## Usage: 
Kmatrixize [1.Input file name] [2.output file name] [3.threshold] [4.method]

method: 0 -> PointMax, 1 -> EdgeMax, 2 -> Random

threshold: minimal contact number for a valid contact

## Example (2~3 min):
Kmatrixize A8.R1.ncc A8.R1.single.point.ncc 2 0

## Require
.net core 6.0: [https://dotnet.microsoft.com/download/dotnet/6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

## Help:

if you have any question, please contact me at: yanmeng(a)sibs.ac.cn. replace (a) with @.
