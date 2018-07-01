#include "StdAfx.h"
#include "VotingSystem.h"


VotingSystem::VotingSystem(void)
{
}


VotingSystem::~VotingSystem(void)
{
}

vector<int>* VotingSystem::WithPreferenceOrders(WithPreferenceOrdersInfo info)
{
	return NULL;
}

vector<int>* VotingSystem::CalculateAdditional(vector<int>* currentSeatCount, vector<double>* nationalSupport)
{
	vector<int>* result = new vector<int>();

	for (int i = 0; i < nationalSupport->size(); i++)
	{
		result->push_back(0);
	}

	return result;
}