// Waywalker.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <SFML/Graphics.hpp>
#include "Character.h"

using namespace std;

int main()
{
	// Make a window that is 800 by 200 pixels
	// And has the title "Waywalker"
	sf::RenderWindow window(sf::VideoMode(1280, 720), "Waywalker");

	sf::Text message;

	Character pc = Character("Hans", "Human", 3);

	sf::Font font;
	font.loadFromFile("Resources\\Fonts\\OptimusPrinceps.ttf");

	message.setFont(font);

	message.setString("Name: " + pc.getName() + "\nMove Speed: " + to_string(pc.getWalkSpeed()));

	message.setCharacterSize(36);

	message.setFillColor(sf::Color::White);

	while (window.isOpen())
	{
		sf::Event event;
		while (window.pollEvent(event))
		{
			if (event.type == sf::Event::Closed)
				window.close();
		}

		// Clear everything from the last run of the while loop
		window.clear();

		// Draw our message
		window.draw(message);

		// Draw our game scene here
		// Just a message for now

		// Show everything we just drew
		window.display();
	}

	return 0;
}

