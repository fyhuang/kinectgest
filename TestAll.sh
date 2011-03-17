#!/bin/bash

mkdir test_results
rm cvindex.txt
for i in {1..10}
do
    mono FinalProject/bin/Debug/FinalProject.exe Train &&
    mono FinalProject/bin/Debug/FinalProject.exe TestRecognize > test_results/test_$i.txt &&
    mono FinalProject/bin/Debug/FinalProject.exe CycleCV
done

for i in {1..10}
do
    echo "CV $i"
    cat test_$i.txt | grep -A 1 'TEST'
done
