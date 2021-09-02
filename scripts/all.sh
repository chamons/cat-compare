#!/bin/bash

for t in `ls ~/Programming/macios/master/xamarin-macios/tests/xtro-sharpie/MacCatalyst-*.todo`; do
	name=`echo $t | grep -o '[^/]*$' | cut -d'.' -f1 | cut -d'-' -f2`
	echo $name
	dotnet run --no-restore --no-build -- -d=/Users/donblas/Programming/macios/master/xamarin-macios/tests/xtro-sharpie -v -s $name | grep 'Orignal\|Removed'
	echo 
done
