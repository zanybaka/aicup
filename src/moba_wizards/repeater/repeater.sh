pushd `dirname $0` > /dev/null
SCRIPTDIR=`pwd`
popd > /dev/null

java -Xms128M -Xmx1G -cp ".:*:$SCRIPTDIR/*" -jar repeater.jar $1