#include "StdAfx.h"
#include "PreferencePredictor.h"
#include "CSVReader.h"


PreferencePredictor::PreferencePredictor(void)
{
	this->allOrders = NULL;
}


PreferencePredictor::~PreferencePredictor(void)
{
	//delete this->conditionalProbability;

	if (allOrders != NULL)
	{
		for (int i = 0; i < this->allOrders->size(); i++)
		{
			delete allOrders->at(i);
		}

		delete allOrders;
	}
}

void PreferencePredictor::Load(wifstream &in, int partyCount)
{
	this->partyCount = _wtoi(readSingle(in).c_str());
	int prefOrdersCount = _wtoi(readSingle(in).c_str());

	this->allOrders = new vector<PreferenceOrder*>();

	for (int i = 0; i < prefOrdersCount; i++)
	{
		PreferenceOrder* pref = new PreferenceOrder();

		vector<String>* line = readCSVLine(in);

		for (int j = 0; j < this->partyCount; j++)
		{
			pref->order->push_back(_wtoi(line->at(j).c_str()));
		}

		pref->weight = _wtof(line->at(this->partyCount).c_str());

		this->allOrders->push_back(pref);
		delete line;
	}

	/*
	this->partyCount = _wtoi(readSingle(in).c_str());

	if (this->partyCount != partyCount)
	{
		wcout << "WARNING: Inconsistant party counts.";
	}

	this->orderCountPerParty = 1;

	for (int i = 0; i < (this->partyCount - 1); i++)
	{
		this->orderCountPerParty *= (i + 1);
	}

	this->conditionalProbability = new double[this->partyCount * this->orderCountPerParty];
	
	for (int i = 0; i < this->partyCount; i++)
	{
		vector<String>* line = readCSVLine(in);

		for (int j = 0; j < this->orderCountPerParty; j++)
		{
			this->conditionalProbability[(i * this->orderCountPerParty) + j] = _wtof(line->at(j).c_str());
		}

		delete line;
	}

	vector<int>* start = new vector<int>();

	this->allOrders = this->_CalculatePreferenceOrders(start);

	delete start;
	*/
}

vector<PreferenceOrder*>* PreferencePredictor::_CalculatePreferenceOrders(vector<int>* start)
{
	vector<PreferenceOrder*>* result = new vector<PreferenceOrder*>();

	if (this->allOrders == NULL)
	{
		for (int i = 0; i < this->partyCount; i++)
		{
			bool contains = false;

			for (int j = 0; j < start->size(); j++)
			{
				if (start->at(j) == i)
				{
					contains = true;
				}
			}

			if (!contains)
			{
				vector<int>* current = new vector<int>();

				for (int j = 0; j < start->size(); j++)
				{
					current->push_back(start->at(j));
				}

				current->push_back(i);

				if (current->size() == this->partyCount)
				{
					PreferenceOrder* po = new PreferenceOrder();
				
					for (int j = 0; j < current->size(); j++)
					{
						po->order->push_back(current->at(j));
					}
				
					result->push_back(po);
				}
				else
				{
					vector<PreferenceOrder*>* xResult = this->_CalculatePreferenceOrders(current);

					for (int j = 0; j < xResult->size(); j++)
					{
						result->push_back(xResult->at(j));
					}	

					delete xResult;
				}

				delete current;
			}
		}
	}
	else
	{
		for (int i = 0; i < this->allOrders->size(); i++)
		{
			PreferenceOrder* old = allOrders->at(i);

			bool hasSameStart = true;

			for (int j = 0; j < min(old->order->size(), start->size()); j++)
			{
				if (old->order->at(j) != start->at(j))
					hasSameStart = false;
			}

			if (hasSameStart)
			{
				PreferenceOrder* pref = new PreferenceOrder();

				for (int j = 0; j < old->order->size(); j++)
				{
					pref->order->push_back(old->order->at(j));
				}

				pref->weight = old->weight;

				result->push_back(pref);
			}
		}
	}

	return result;
}

vector<PreferenceOrder*>* PreferencePredictor::CalculatePreferenceOrders(int partyIndex, double supportWeight)
{
	vector<int>* start = new vector<int>();

	start->push_back(partyIndex);

	vector<PreferenceOrder*>* result = this->_CalculatePreferenceOrders(start);

	delete start;

	double weightTotal = 0.0;

	for (int i = 0; i < result->size(); i++)
		weightTotal += result->at(i)->weight;

	for (int i = 0; i < result->size(); i++)
	{
		result->at(i)->weight *= (supportWeight / weightTotal);
	}

	return result;
}