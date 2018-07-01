#pragma once
#include "PreferenceOrder.h"

class PreferencePredictor
{
public:
	PreferencePredictor(void);
	~PreferencePredictor(void);

	vector<PreferenceOrder*>* CalculatePreferenceOrders(int partyIndex, double supportWeight);
	void Load(wifstream &in, int partyCount);
private:
	int partyCount;
	int orderCountPerParty;
	double* conditionalProbability;
	vector<PreferenceOrder*>* _CalculatePreferenceOrders(vector<int>* start);
	vector<PreferenceOrder*>* allOrders;
};

