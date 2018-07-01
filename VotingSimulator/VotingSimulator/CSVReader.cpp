#pragma once
#include "stdafx.h"
#include "CSVReader.h"

String readSingle(wifstream &in)
{
	vector<String>* line = readCSVLine(in);
	String result;

	// Check file header.
	if (line->size() >= 1)
	{
		result = line->front();
	}
	else
	{
		result = L"";
	}

	delete line;

	return result;
}

map<String, String>* getHeaderTuples(vector<String>* headers, vector<String>* values)
{
	if (headers == NULL || values == NULL)
	{
		return NULL;
	}

	int count = headers->size() < values->size() ? headers->size() : values->size();

	map<String, String>* result = new map<String, String>();

	for (int i = 0; i < count; i++)
	{
		String header = headers->at(i);
		String value = values->at(i);

		if (header.length() > 0)
		{
			result->insert(pair<String, String>(header, value));
		}
	}

	return result;
}

map<String, String>* readHeaderTuples(wifstream &in)
{
	vector<String>* headers = readCSVLine(in);
	vector<String>* values = readCSVLine(in);

	map<String, String>* result = getHeaderTuples(headers, values);

	delete headers;
	delete values;

	return result;
}

vector<String>* readCSVLine(wifstream &in)
{	
	
	String item;

	if (!getline(in, item))
	{
		return NULL;
	}
	
	vector<String>* line = new vector<String>();

	int pos = 0;

	while(true)
	{
		int nextPos = item.find(',', pos);

		if (nextPos >= 0)
		{
			line->push_back(item.substr(pos, nextPos - pos));
			pos = nextPos + 1;
		}
		else
		{
			line->push_back(item.substr(pos, item.length() - pos));
			break;
		}
	}

	return line;
}

int getIndexOf(vector<String>* v, String value)
{
	for (int i = 0; i < v->size(); i++)
	{
		if (v->at(i).compare(value) == 0)
		{
			return i;
		}
	}

	return -1;
}