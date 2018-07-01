#pragma once
#include "stdafx.h"
#include "Party.h"
#include "PreferenceOrder.h"

struct WithPreferenceOrdersInfo
{
	vector<PreferenceOrder*>* prefOrders;
	int partyCount;
	int expectedWinnerCount;
};

class VotingSystem
{
public:
	VotingSystem(void);
	virtual ~VotingSystem();
	virtual int Load(wifstream &in, int partyCount, map<String, String>* customMetaData) = 0;
	virtual int PrintResults() = 0;

	// Calculates winners per electorate.
	virtual vector<int>* WithPreferenceOrders(WithPreferenceOrdersInfo info);

	// Calculates the number of additional seats per party to add, given the current seat count and national support level.
	virtual vector<int>* CalculateAdditional(vector<int>* currentSeatCount, vector<double>* nationalSupport);
};

