#pragma once

class VotePredictor
{
public:
	VotePredictor(void);
	~VotePredictor(void);

	virtual vector<double>* CalculateFirstPreferencesForParties(int electorateIndex, vector<double>* overallSupport) = 0;
};

