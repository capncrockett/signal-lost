// Define the structure of a message
export interface Message {
  id: string;
  title: string;
  content: string;
  sender?: string;
  timestamp?: string;
  coordinates?: string;
  isDecoded: boolean;
  decodedContent?: string;
  requiredProgress?: number; // Game progress required to fully decode this message
}

// Define the messages that can be received
export const messages: Record<string, Message> = {
  intro_signal: {
    id: 'intro_signal',
    title: 'First Contact',
    content: 'This is Station Alpha. If anyone can hear this, please respond. We\'ve lost contact with the main facility. Coordinates follow: 37.4419° N, 122.1430° W.',
    sender: 'Dr. Sarah Chen',
    timestamp: '2023-06-15 08:42',
    coordinates: '37.4419° N, 122.1430° W',
    isDecoded: true,
    requiredProgress: 0,
  },
  distress_call: {
    id: 'distress_call',
    title: 'Distress Call',
    content: 'Mayday, mayday! This is Research Outpost Delta. We are under attack by unknown entities. They came from the [static] and we can\'t [static] them. If anyone receives this, stay away from [static].',
    sender: 'Security Officer Rodriguez',
    timestamp: '2023-06-15 09:17',
    isDecoded: false,
    decodedContent: 'Mayday, mayday! This is Research Outpost Delta. We are under attack by unknown entities. They came from the forest and we can\'t contain them. If anyone receives this, stay away from the northern sector.',
    requiredProgress: 2,
  },
  coordinates: {
    id: 'coordinates',
    title: 'Coordinates Transmission',
    content: 'Field team, rendezvous at the following coordinates: 37.4275° N, 122.1697° W. Bring the specialized equipment. Do not approach without proper protection.',
    sender: 'Field Commander Taylor',
    timestamp: '2023-06-15 10:03',
    coordinates: '37.4275° N, 122.1697° W',
    isDecoded: true,
    requiredProgress: 0,
  },
  warning: {
    id: 'warning',
    title: 'Warning Broadcast',
    content: '[static] warning to all personnel. The anomaly is [static] expanding. Evacuation protocols [static] initiated. Proceed to [static] immediately.',
    sender: 'Emergency Broadcast System',
    timestamp: '2023-06-15 11:30',
    isDecoded: false,
    decodedContent: 'Urgent warning to all personnel. The anomaly is rapidly expanding. Evacuation protocols have been initiated. Proceed to extraction point Alpha immediately.',
    requiredProgress: 3,
  },
  final_message: {
    id: 'final_message',
    title: 'Final Transmission',
    content: 'This is Dr. Chen. I\'ve discovered what they were hiding. The experiment wasn\'t just about [static]. It was about opening a [static]. I\'ve downloaded the data to my personal device. Find me at the old [static] tower.',
    sender: 'Dr. Sarah Chen',
    timestamp: '2023-06-15 13:45',
    isDecoded: false,
    decodedContent: 'This is Dr. Chen. I\'ve discovered what they were hiding. The experiment wasn\'t just about signal amplification. It was about opening a gateway. I\'ve downloaded the data to my personal device. Find me at the old radio tower.',
    requiredProgress: 4,
  },
};

// Function to get a message by ID
export const getMessage = (id: string): Message | undefined => {
  return messages[id];
};

// Function to get a partially decoded message based on game progress
export const getDecodedMessage = (
  message: Message,
  gameProgress: number
): string => {
  if (message.isDecoded || !message.decodedContent) {
    return message.content;
  }
  
  // If the player has enough progress, return the fully decoded message
  if (gameProgress >= (message.requiredProgress || 0)) {
    return message.decodedContent;
  }
  
  // Otherwise, return a partially decoded message
  const originalWords = message.content.split(' ');
  const decodedWords = message.decodedContent.split(' ');
  
  // Calculate how much of the message to decode based on progress
  const decodeRatio = gameProgress / (message.requiredProgress || 1);
  const decodedWordCount = Math.floor(decodedWords.length * decodeRatio);
  
  // Replace some static with decoded words
  const result = [...originalWords];
  
  // Randomly select words to decode
  const indicesToDecode = new Set<number>();
  while (indicesToDecode.size < decodedWordCount) {
    const randomIndex = Math.floor(Math.random() * originalWords.length);
    if (originalWords[randomIndex].includes('[static]')) {
      indicesToDecode.add(randomIndex);
    }
  }
  
  // Replace the selected words with decoded versions
  indicesToDecode.forEach(index => {
    result[index] = decodedWords[index];
  });
  
  return result.join(' ');
};
