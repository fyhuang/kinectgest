#!/usr/bin/python
import sys
import glob
import os.path

import re
import shutil


out_template = "gestures/track_{}_{:02}.log"
def findLast(gname):
    for i in range(100):
        if not os.path.exists(out_template.format(gname, i)):
            return i-1

# MAIN
lasts = {}
input_dir = sys.argv[1]

fname_re = re.compile(os.path.join(input_dir, 'track_(?P<gname>\w+)_(?P<gnum>\d\d).log'))
files = glob.glob(os.path.join(input_dir, 'track_*_[0-9][0-9].log'))
for filename in files:
    result = fname_re.match(filename)
    if result:
        gname = result.group('gname')
        gnum = int(result.group('gnum'))

        if gname not in lasts:
            lasts[gname] = findLast(gname)
        lasts[gname] += 1

        out_name = out_template.format(gname, lasts[gname])
        if os.path.exists(out_name):
            print("Error: file {} already exists!".format(out_name))
            sys.exit()
        shutil.move(filename, out_name)
        print("Moved {} to {}".format(filename, out_name))
    else:
        print("Extraneous file {}".format(filename))
