#pragma once
#include "stdafx.h"

int getIndexOf(vector<String>* v, String value);
vector<String>* readCSVLine(wifstream &in);
map<String, String>* readHeaderTuples(wifstream &in);
map<String, String>* getHeaderTuples(vector<String>* headers, vector<String>* values);
String readSingle(wifstream &in);
