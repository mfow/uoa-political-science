#pragma once
#include "votingsystem.h"

enum ApprovalOptions
{
	Approval,			// (or Raw mode) Expects rows of weights for each candidate. (either one or zero, or some other value if allowed)
	Borda,				// Expects rows of the index to the most prefered candidate starting from the left.
						// Automatically converts this to raw weightings.
};

struct ApprovalVote
{
	int* votes;
};

struct ApprovalCandidate
{
	int partyIndex;
	int votes;
	map<String, String>* properties;
};

struct ApprovalElectorate
{
	int candidateCount;
	ApprovalCandidate* candidates;
	map<String, String>* properties;
	vector<ApprovalVote>* votes;
};

class ApprovalVotingSystem :
	public VotingSystem
{
public:
	ApprovalVotingSystem(ApprovalOptions options);
	~ApprovalVotingSystem(void);
	int ApprovalVotingSystem::Load(wifstream &in, int partyCount, map<String, String>* customMetaData);
	int ApprovalVotingSystem::PrintResults();
private:
	int electorateCount;
	ApprovalElectorate* electorates;
	ApprovalOptions options;
};

