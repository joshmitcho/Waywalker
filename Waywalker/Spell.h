#include <string>
#pragma once

class Spell
{
public:
	Spell();
	~Spell();

private:
	int damage;
	std::string school;
	void secondaryEffects();
};

