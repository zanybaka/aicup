name="MyStrategy"

if [ ! -f $name.cs ]
then
    echo Unable to find $name.cs > compilation.log
    exit 1
fi

rm -f $name.exe

files=""

for i in *.cs
do
    files="$files $i"
done

for i in Model/*.cs
do
    files="$files $i"
done

for i in Properties/*.cs
do
    files="$files $i"
done

dmcs -o+ /r:System.Numerics.dll -sdk:6 -out:$name.exe $files 2>compilation.log
