#pragma once
#include "votingsystem.h"

enum PVOptions
{
	PreferentialVoting,
	Bucklin,
};

struct PVVote
{
	int* votes;
};

struct PVCandidate
{
	int partyIndex;
	int votes;
	bool isExcluded;
	map<String, String>* properties;
};

struct PVElectorate
{
	int candidateCount;
	PVCandidate* candidates;
	map<String, String>* properties;
	vector<PVVote>* votes;
};

class PVVotingSystem :
	public VotingSystem
{
public:
	PVVotingSystem(PVOptions options);
	~PVVotingSystem(void);
	int PVVotingSystem::Load(wifstream &in, int partyCount, map<String, String>* customMetaData);
	int PVVotingSystem::PrintResults();
	vector<int>* PVVotingSystem::WithPreferenceOrders(WithPreferenceOrdersInfo info);
private:
	int electorateCount;
	PVElectorate* electorates;
	PVOptions options;
};

