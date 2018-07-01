#pragma once
#include "votingsystem.h"

enum CondorcetOptions
{
	Copelands,
	Minimax,

};

struct CondorcetVote
{
	int* votes;
	double weight;
};

struct CondorcetCandidate
{
	int partyIndex;
	int votes;
	bool isExcluded;
	map<String, String>* properties;
};

struct CondorcetElectorate
{
	int candidateCount;
	CondorcetCandidate* candidates;
	map<String, String>* properties;
	vector<CondorcetVote>* votes;
};

class CondorcetVotingSystem :
	public VotingSystem
{
public:
	CondorcetVotingSystem(CondorcetOptions options);
	~CondorcetVotingSystem(void);
	int CondorcetVotingSystem::Load(wifstream &in, int partyCount, map<String, String>* customMetaData);
	int CondorcetVotingSystem::PrintResults();
	vector<int>* CondorcetVotingSystem::WithPreferenceOrders(WithPreferenceOrdersInfo info);
private:
	int electorateCount;
	CondorcetElectorate* electorates;
	CondorcetOptions options;
	vector<int>* CondorcetVotingSystem::calculateElectorateWinners(CondorcetElectorate electorate);
};

