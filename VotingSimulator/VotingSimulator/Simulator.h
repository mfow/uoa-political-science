#pragma once


#include "VotingSystem.h"
#include "VotePredictor.h"
#include "PreferencePredictor.h"

class Simulator
{
public:
	Simulator(void);
	~Simulator(void);

	int partyCount;
	int electorateCount;

	VotingSystem* votingSystem;
	vector<double>* pollResults;
	vector<String>* partyNames;
	VotePredictor* predictor;
	PreferencePredictor* prefPredictor;
	vector<int>* electorateSeats;

	vector<int>* Simulate();
};

