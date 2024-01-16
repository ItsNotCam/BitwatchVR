# BitwatchVR

NOTE: **There is only one commit with code in this repository** due to the fact that the official development of this application and the GitHub account associated with it is followed by an online community; I do not wish to directly use my professional identity online

![Watch](https://github.com/ItsNotCam/BitwatchVR/assets/46014191/1a034f9b-c3a5-4184-ac9a-b82b98a1ef77)

## Introduction
BitwatchVR is an application designed to **establish a seamless connection between the virtual world and the real world**. The primary goal of this project is to **allow users to stay informed about their real-world surroundings** without disrupting their virtual experiences.

## Features
**The following information is displayed in real time**
- Time and Date
- Heart Rate Monitoring
- Weather and Temperature
- User's Controller Battery Level

## Challenges
- **Limited Data Transfer Rate:** The platform that this software interacts with imposes a **1-10hz limitation on data transfer rate** (this transfer rate is dependent on certain in-game factors that are out of my control).
- **16-Bit Data Representation:** Only 16 bits may be used to represent all the information listed above.

## Solutions
- **Multi-threaded Processing:**
  - To address the **limited data transfer rate**, the application utilizes multiple **threads**, each assigned to a specific information category. 
  - This approach enables fine-grained control over local data retrieval rates for each category, ensuring the information remains as up-to-date as possible.
  - The application then sends this data at the limited rate

- **Time Division Multiplexing:** 
  - To overcome the **16-bit data representation** limitation, **time division multiplexing** is employed. 
  - The **first eight bits** are used to inform the platform about the **type of information being sent**.
  - The **remaining eight bits** are used to **represent the actual data** being transmitted.
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
