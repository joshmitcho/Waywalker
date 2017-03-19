#include <string>
#pragma once

class Character
{
public:
	Character(std::string name, std::string race, int lvl);
	~Character();
	std::string getName();
	int getWalkSpeed();
	void setStats(int hp, int str, int dex, int con, int intell, int wis, int cha);

private:
	std::string name;
	std::string race;
	int walkSpeed;
	int level;
	int exp;

	//stats
	int hp;
	int ac;
	int strength;
	int dexterity;
	int constitution;
	int intelligence;
	int wisdom;
	int charisma;

	//saves
	int fortSave;
	int reflexSave;
	int willSave;

	void setWalkSpeed();
};

