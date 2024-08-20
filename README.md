# ⌚ BitwatchVR

## Introduction
BitwatchVR is an application designed to establish a seamless connection between the virtual world and the real world. The primary goal of this project is to allow users to stay informed about their real-world surroundings without disrupting their virtual experiences. To solve this, **BitwatchVR displays pertinent real-world information on a watch that is worn on the user's virtual wrist** (displayed below). **Scroll down for the technology stack that I used and for more information.**

**The Watch, UI, and the front and back end software is all created from scratch by me.**

NOTE: **There is only one commit with code in this repository** due to the fact that the official development of this application and the GitHub account associated with it is followed by a large online community; I do not wish to directly use my professional identity online


## Displaying the Time and Weather to the User
This information is stored on multiple "widgets" that are selectable by the user
![image](https://github.com/ItsNotCam/BitwatchVR/assets/46014191/4f8ae9a8-53a9-40f1-89e6-c566023a90de)

## Features
**The following information is displayed in real time**
- Time and Date
- Heart Rate Monitoring
- Weather and Temperature
- User's Controller Battery Level

## Challenges and Solutions
- **Real Time Updating:** The information that is displayed to the user must be able to be retrieved anywhere from 1 to 10 times per second, and only one piece of information can be sent at any given time. Data retrieval creates a massive bottleneck here, as it requires several seconds to retrieve all of the necessary information.
  - The application utilizes multiple **threads**, each assigned to a specific information category (time, weather, heart rate, battery levels, etc.). Each thread is responsible for asyncronously retrieving its category of data, which the main thread then reads from.
  - This allows the time-consuming portion of data retrieval to be handled separately from the main thread that communicates with the client.

- **16-Bit Data Representation:** Only 16 *total* bits may be used to represent all of the information listed above.
  - To overcome this, I made use of a technique called "**time division multiplexing (TDM)**"
  - TDM allows me to send each category of data sequentially in its own encapsulated "message", each containing 16 bits of data.
  - The **first 8 bits** of each message are used to inform the platform about the **type of information being sent**, and the **remaining 8 bits** of each message are used to **represent the actual data** being transmitted.
  - This information is then decoded and temporarily stored by the client until it is updated.
  - This technique allows efficient representation of diverse real-world information within the constraints of the 16-bit limit.

## Technology Stack
- **C#:** I wanted to develop this project in C# in order to learn more about the language.
- **Web and OSC Sockets:** Web and OSC (Open Sound Control) sockets facilitate communication between the application and the platform, ensuring real-time data exchange.
- **REST APIs:** RESTful APIs are used to fetch real-world data
- **Multithreaded Processing:** The C# Threading library is utilized for multi-threaded processing, allowing for effective management of data retrieval rates.

## What I Learned
Throughout the development of this application I:
* Deepened my understanding of C#.
* Worked with and developed my own state machines.
* Became familiar with multithreaded processing.
* Overcame many technical challenges by using self-taught techniques through research.

Overall this project was very helpful in furthering my knowledge in many areas, and is undoubtedly the project that I am the most proud of to date (2024).
