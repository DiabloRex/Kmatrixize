# Kmatrixize
Covert populational hic ncc format (nuc_process) matrix to Genome Khimaira matrix

## Usage: 
Kmatrixize [1.Input file] [2.Output file] [3.Threshold] [4.Method]

Method: 0 -> PointMax [default], 1 -> EdgeMax, 2 -> Random

Threshold: [default -> 0]; for PointMax -> lowest number of contact (edge) number for each point (segments); for EdgeMax -> lowest number of contact (edge) number for each point (segments) pair;

## Example (2~3 min):
Kmatrixize A8.R1.ncc A8.R1.kmatrix.point.ncc 2 0

## Require
.net core 6.0: [https://dotnet.microsoft.com/download/dotnet/6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

## Help:

if you have any question, please contact me at: yanmeng(a)sibs.ac.cn. replace (a) with @.
