#include "StdAfx.h"
#include "CondorcetVotingSystem.h"
#include "CSVReader.h"

CondorcetVotingSystem::CondorcetVotingSystem(CondorcetOptions options)
{
	this->options = options;
}


CondorcetVotingSystem::~CondorcetVotingSystem(void)
{
}

int CondorcetVotingSystem::Load(wifstream &in, int partyCount, map<String, String>* customMetaData)
{
	vector<String>* partyHeaders = readCSVLine(in);

	for (int h = 0; h < partyCount; h++)
	{
		vector<String>* partyData = readCSVLine(in);

		delete partyData;
	}

	delete partyHeaders;

	this->electorateCount = _wtoi(readSingle(in).c_str());
	this->electorates = new CondorcetElectorate[CondorcetVotingSystem::electorateCount];

	for (int i = 0; i < CondorcetVotingSystem::electorateCount; i++)
	{
		CondorcetElectorate electorate;
		electorate.candidateCount = _wtoi(readSingle(in).c_str());
		electorate.candidates = new CondorcetCandidate[electorate.candidateCount];
		electorate.votes = new vector<CondorcetVote>;

		electorate.properties = readHeaderTuples(in);

		vector<String>* candidateHeaders = readCSVLine(in);

		int partyHeaderIndex = getIndexOf(candidateHeaders, L"Party");

		for (int j = 0; j < electorate.candidateCount; j++)
		{
			CondorcetCandidate candidate;

			vector<String>* candidateData = readCSVLine(in);

			candidate.properties = getHeaderTuples(candidateHeaders, candidateData);
			candidate.partyIndex = _wtoi(candidateData->at(partyHeaderIndex).c_str());
			candidate.votes = 0;
			candidate.isExcluded = false;

			electorate.candidates[j] = candidate;

			delete candidateData;
		}

		delete candidateHeaders;

		this->electorates[i] = electorate;

		int numberOfVotesInElectorate = _wtoi(readSingle(in).c_str());

		vector<String>* voteHeaders = readCSVLine(in);
		int voteHeaderIndex = getIndexOf(voteHeaders, L"Vote");

		for (int k = 0; k < numberOfVotesInElectorate; k++)
		{
			vector<String>* voteData = readCSVLine(in);

			CondorcetVote vote;
			vote.votes = new int[electorate.candidateCount];

			for (int m = 0; m < electorate.candidateCount; m++)
			{
				int voteIndex = _wtoi(voteData->at(voteHeaderIndex + m).c_str());

				vote.votes[m] = voteIndex;
				vote.weight = 1.0;
			}

			electorate.votes->push_back(vote);

			delete voteData;
		}

		delete voteHeaders;
	}

	return 0;
}

vector<int>* CondorcetVotingSystem::calculateElectorateWinners(CondorcetElectorate electorate)
{
	vector<int>* winners = new vector<int>();

	double* winsMatrix = new double[electorate.candidateCount * electorate.candidateCount];

	// Reset vote count.
	for (int j = 0; j < electorate.candidateCount; j++)
	{
		electorate.candidates[j].votes = 0;
	}

	int x;
	int y;

	// Reset matrix.
	for (x = 0; x < electorate.candidateCount; x++)
	{
		for (y = 0; y < electorate.candidateCount; y++)
		{
			winsMatrix[x * electorate.candidateCount + y] = 0;
		}
	}

	// Calculate matrix of preference wins.
	for (int j = 0; j < electorate.votes->size(); j++)
	{
		CondorcetVote vote = electorate.votes->at(j);

		for (x = 0; x < electorate.candidateCount; x++)
		{
			for (y = x + 1; y < electorate.candidateCount; y++)
			{
				winsMatrix[vote.votes[x] * electorate.candidateCount + vote.votes[y]] += vote.weight;
			}
		}
	}

	// Calculate candidate wins - losses
	for (x = 0; x < electorate.candidateCount; x++)
	{
		for (y = x + 1; y < electorate.candidateCount; y++)
		{
			if (winsMatrix[x * electorate.candidateCount + y] >
				winsMatrix[y * electorate.candidateCount + x])
			{
				electorate.candidates[x].votes++;
				electorate.candidates[y].votes--;
			}
			else
			{
				electorate.candidates[x].votes--;
				electorate.candidates[y].votes++;
			}
		}
	}

	int winningIndex = -1;
	double winningVoteCount = INT_MIN;
	double* pairwiseScores;

	switch (this->options)
	{
		case CondorcetOptions::Copelands:
			// Calculate candidate with the most wins - losses.
			for (int j = 0; j < electorate.candidateCount; j++)
			{
				int candidateVotes = electorate.candidates[j].votes;

				if (candidateVotes > winningVoteCount)
				{
					winningIndex = j;
					winningVoteCount = candidateVotes;
				}
			}

			for (int j = 0; j < electorate.candidateCount; j++)
			{
				int candidateVotes = electorate.candidates[j].votes;

				if (candidateVotes == winningVoteCount)
				{
					winners->push_back(j);
				}
			}
			break;
		case CondorcetOptions::Minimax:
			// Iterate through each candidate. Select the candidate who's worst score against any other candidate is the least bad.

			pairwiseScores = new double[electorate.candidateCount];

			for (x = 0; x < electorate.candidateCount; x++)
			{
				double worstPairwise = INT_MAX;

				for (y = 0; y < electorate.candidateCount; y++)
				{
					if (x != y)
					{
						double pairwiseScore = winsMatrix[x * electorate.candidateCount + y] - winsMatrix[y * electorate.candidateCount + x];

						if (pairwiseScore < worstPairwise)
						{
							worstPairwise = pairwiseScore;
						}
					}
				}

				pairwiseScores[x] = worstPairwise;

				if (winningVoteCount < worstPairwise)
				{
					winningIndex = x;
					winningVoteCount = worstPairwise;
				}
			}

			for (x = 0; x < electorate.candidateCount; x++)
			{
				if (pairwiseScores[x] == winningVoteCount)
					winners->push_back(x);
			}

			delete pairwiseScores;

			break;
		default:
			wcout << L"Unknown options." << endl;
	}
		
	delete winsMatrix;

	return winners;
}

int CondorcetVotingSystem::PrintResults()
{
	wcout << L"Election type: Condorcet" << endl;
	wcout << L"Electorate count: " << this->electorateCount << endl;

	for (int i = 0; i < this->electorateCount; i++)
	{
		CondorcetElectorate electorate = this->electorates[i];

		vector<int>* winners = this->calculateElectorateWinners(electorate);

		int winningIndex = winners->at(0);

		wcout << L"Electorate " << i << endl;
		wcout << L"Winner " << electorate.candidates[winningIndex].properties->at(L"Name") << endl << endl;

		delete winners;
	}

	return 0;
}

vector<int>* CondorcetVotingSystem::WithPreferenceOrders(WithPreferenceOrdersInfo info)
{
	vector<PreferenceOrder*>* prefOrders = info.prefOrders;
	int partyCount = info.partyCount;

	CondorcetElectorate electorate;

	electorate.candidateCount = partyCount;
	electorate.candidates = new CondorcetCandidate[partyCount];
	electorate.votes = new vector<CondorcetVote>();

	for (int i = 0; i < partyCount; i++)
	{
		CondorcetCandidate cand;

		cand.isExcluded = false;
		cand.partyIndex = i;
		cand.votes = 0;
		
		electorate.candidates[i] = cand;
	}

	for (int i = 0; i < prefOrders->size(); i++)
	{
		PreferenceOrder* pref = prefOrders->at(i);
		CondorcetVote vote;

		vote.weight = pref->weight;
		vote.votes = new int[partyCount];

		for (int j = 0; j < partyCount; j++)
		{
			vote.votes[j] = pref->order->at(j);
		}

		electorate.votes->push_back(vote);
	}

	vector<int>* result = this->calculateElectorateWinners(electorate);


	for (int i = 0; i < electorate.votes->size(); i++)
	{
		delete electorate.votes->at(i).votes;
	}

	delete electorate.votes;
	delete electorate.candidates;

	return result;
}