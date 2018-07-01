#include "StdAfx.h"
#include "ApprovalVotingSystem.h"
#include "CSVReader.h"

ApprovalVotingSystem::ApprovalVotingSystem(ApprovalOptions options)
{
	this->options = options;
}


ApprovalVotingSystem::~ApprovalVotingSystem(void)
{
}


int ApprovalVotingSystem::Load(wifstream &in, int partyCount, map<String, String>* customMetaData)
{
	vector<String>* partyHeaders = readCSVLine(in);

	for (int h = 0; h < partyCount; h++)
	{
		vector<String>* partyData = readCSVLine(in);

		delete partyData;
	}

	delete partyHeaders;

	this->electorateCount = _wtoi(readSingle(in).c_str());
	this->electorates = new ApprovalElectorate[ApprovalVotingSystem::electorateCount];

	for (int i = 0; i < ApprovalVotingSystem::electorateCount; i++)
	{
		ApprovalElectorate electorate;
		electorate.candidateCount = _wtoi(readSingle(in).c_str());
		electorate.candidates = new ApprovalCandidate[electorate.candidateCount];
		electorate.votes = new vector<ApprovalVote>;

		electorate.properties = readHeaderTuples(in);

		vector<String>* candidateHeaders = readCSVLine(in);

		int partyHeaderIndex = getIndexOf(candidateHeaders, L"Party");

		for (int j = 0; j < electorate.candidateCount; j++)
		{
			ApprovalCandidate candidate;

			vector<String>* candidateData = readCSVLine(in);

			candidate.properties = getHeaderTuples(candidateHeaders, candidateData);
			candidate.partyIndex = _wtoi(candidateData->at(partyHeaderIndex).c_str());
			candidate.votes = 0;

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

			ApprovalVote vote;
			vote.votes = new int[electorate.candidateCount];

			switch (this->options)
			{
				case ApprovalOptions::Approval:
					for (int m = 0; m < electorate.candidateCount; m++)
					{
						int voteIndex = _wtoi(voteData->at(voteHeaderIndex + m).c_str());

						vote.votes[m] = voteIndex;
					}
					break;
				case ApprovalOptions::Borda:
					for (int m = 0; m < electorate.candidateCount; m++)
					{
						int candidatePreferenceIndex = _wtoi(voteData->at(voteHeaderIndex + m).c_str());

						vote.votes[candidatePreferenceIndex] = electorate.candidateCount - m;
					}
					break;
				default:
					wcout << L"Unknown options." << endl;
					return -1;
			}

			electorate.votes->push_back(vote);

			delete voteData;
		}

		delete voteHeaders;
	}

	return 0;
}

int ApprovalVotingSystem::PrintResults()
{
	wcout << L"Election type: Approval" << endl;
	wcout << L"Electorate count: " << this->electorateCount << endl;

	for (int i = 0; i < this->electorateCount; i++)
	{
		ApprovalElectorate electorate = this->electorates[i];

		wcout << L"Electorate " << i << endl;
		
		int majorityCutoff = electorate.votes->size() / 2;

		int winningIndex = -1;
		int winningVoteCount = -1;

		// Reset vote count for electorate.
		for (int j = 0; j < electorate.candidateCount; j++)
		{
			electorate.candidates[j].votes = 0;
		}

		// Count votes.
		for (int j = 0; j < electorate.votes->size(); j++)
		{
			ApprovalVote vote = electorate.votes->at(j);

			for (int k = 0; k < electorate.candidateCount; k++)
			{
				electorate.candidates[j].votes += vote.votes[k];
			}
		}

		// Calculate winner for this electorate.
		for (int j = 0; j < electorate.candidateCount; j++)
		{
			int candidateVotes = electorate.candidates[j].votes;

			if (candidateVotes > winningVoteCount)
			{
				winningIndex = j;
				winningVoteCount = candidateVotes;
			}
		}

		wcout << L"Winner " << electorate.candidates[winningIndex].properties->at(L"Name") << L" " << winningVoteCount << L" votes" << endl << endl;
	}

	return 0;
}