#include "StdAfx.h"
#include "PreferenceOrder.h"


PreferenceOrder::PreferenceOrder(void)
{
	this->order = new vector<int>();

}


PreferenceOrder::~PreferenceOrder(void)
{
	delete this->order;
}
