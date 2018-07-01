#pragma once

#include "VotePredictor.h"

class DefaultVotePredictor : 
	public VotePredictor
{
public:
	DefaultVotePredictor(void);
	~DefaultVotePredictor(void);

	vector<double>* DefaultVotePredictor::CalculateFirstPreferencesForParties(int electorateIndex, vector<double>* overallSupport);
	void Load(wifstream &in, int partyCount);
	int electorateCount;
private:
	
	int partyCount;
	double* spatialProbability;
};

