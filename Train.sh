#!/bin/bash

mono --debug FinalProject/bin/Debug/FinalProject.exe Train | tee training.log &&
echo >> training_hist.log &&
date >> training_hist.log &&
cat training.log >> training_hist.log
