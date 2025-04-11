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
    content:
      "This is Station Alpha. If anyone can hear this, please respond. We've lost contact with the main facility. Coordinates follow: 37.4419° N, 122.1430° W.",
    sender: 'Dr. Sarah Chen',
    timestamp: '2023-06-15 08:42',
    coordinates: '37.4419° N, 122.1430° W',
    isDecoded: true,
    requiredProgress: 0,
  },
  distress_call: {
    id: 'distress_call',
    title: 'Distress Call',
    content:
      "Mayday, mayday! This is Research Outpost Delta. We are under attack by unknown entities. They came from the [static] and we can't [static] them. If anyone receives this, stay away from [static].",
    sender: 'Security Officer Rodriguez',
    timestamp: '2023-06-15 09:17',
    isDecoded: false,
    decodedContent:
      "Mayday, mayday! This is Research Outpost Delta. We are under attack by unknown entities. They came from the forest and we can't contain them. If anyone receives this, stay away from the northern sector.",
    requiredProgress: 2,
  },
  coordinates: {
    id: 'coordinates',
    title: 'Coordinates Transmission',
    content:
      'Field team, rendezvous at the following coordinates: 37.4275° N, 122.1697° W. Bring the specialized equipment. Do not approach without proper protection.',
    sender: 'Field Commander Taylor',
    timestamp: '2023-06-15 10:03',
    coordinates: '37.4275° N, 122.1697° W',
    isDecoded: true,
    requiredProgress: 0,
  },
  warning: {
    id: 'warning',
    title: 'Warning Broadcast',
    content:
      '[static] warning to all personnel. The anomaly is [static] expanding. Evacuation protocols [static] initiated. Proceed to [static] immediately.',
    sender: 'Emergency Broadcast System',
    timestamp: '2023-06-15 11:30',
    isDecoded: false,
    decodedContent:
      'Urgent warning to all personnel. The anomaly is rapidly expanding. Evacuation protocols have been initiated. Proceed to extraction point Alpha immediately.',
    requiredProgress: 3,
  },
  final_message: {
    id: 'final_message',
    title: 'Final Transmission',
    content:
      "This is Dr. Chen. I've discovered what they were hiding. The experiment wasn't just about [static]. It was about opening a [static]. I've downloaded the data to my personal device. Find me at the old [static] tower.",
    sender: 'Dr. Sarah Chen',
    timestamp: '2023-06-15 13:45',
    isDecoded: false,
    decodedContent:
      "This is Dr. Chen. I've discovered what they were hiding. The experiment wasn't just about signal amplification. It was about opening a gateway. I've downloaded the data to my personal device. Find me at the old radio tower.",
    requiredProgress: 4,
  },
  // New messages for Sprint 3
  research_log: {
    id: 'research_log',
    title: 'Research Log Entry #42',
    content:
      "Research Log, Entry 42. The [static] experiments have shown promising results. We've managed to [static] the signal by nearly 300%. Dr. Chen believes we can [static] even further with the new [static] we've developed.",
    sender: 'Dr. Marcus Wei',
    timestamp: '2023-06-14 15:22',
    isDecoded: false,
    decodedContent:
      "Research Log, Entry 42. The resonance experiments have shown promising results. We've managed to amplify the signal by nearly 300%. Dr. Chen believes we can push the boundaries even further with the new quantum receiver we've developed.",
    requiredProgress: 2,
  },
  encrypted_coordinates: {
    id: 'encrypted_coordinates',
    title: 'Encrypted Coordinates',
    content:
      '01001110 00110111 01010111 00110001 00110010 00110011 00101110 00110100 00110101 00110110 00101100 01010111 00110001 00110010 00110011 00101110 00110100 00110101 00110110',
    sender: 'Unknown',
    timestamp: '2023-06-15 02:37',
    isDecoded: false,
    decodedContent: 'N37°12.456,W122°34.567',
    requiredProgress: 5,
  },
  survivors_log: {
    id: 'survivors_log',
    title: "Survivor's Log",
    content:
      "Day 3 after the [static]. Food supplies are [static]. We've barricaded ourselves in the [static] lab, but I don't know how long we can [static]. If anyone finds this, the [static] is in the lower levels. Don't [static] it.",
    sender: 'Dr. Emily Nakamura',
    timestamp: '2023-06-16 07:19',
    isDecoded: false,
    decodedContent:
      "Day 3 after the incident. Food supplies are running low. We've barricaded ourselves in the secondary lab, but I don't know how long we can hold out. If anyone finds this, the containment breach is in the lower levels. Don't approach it.",
    requiredProgress: 3,
  },
  military_transmission: {
    id: 'military_transmission',
    title: 'Military Transmission',
    content:
      "This is Captain [static] of the Special [static] Unit. We've been ordered to [static] the facility and [static] all evidence of the [static]. All civilian personnel are to be [static] immediately. Authorization code: [static].",
    sender: 'Captain [Redacted]',
    timestamp: '2023-06-16 13:05',
    isDecoded: false,
    decodedContent:
      "This is Captain Reynolds of the Special Containment Unit. We've been ordered to secure the facility and destroy all evidence of the experiment. All civilian personnel are to be evacuated immediately. Authorization code: BLACKLIGHT-7.",
    requiredProgress: 4,
  },
  personal_note: {
    id: 'personal_note',
    title: 'Personal Note',
    content:
      "If you're reading this, I might already be [static]. I've hidden the [static] in my personal locker. The code is [static]. Whatever you do, don't let them [static] what we've discovered. It could change [static] forever.",
    sender: 'Dr. Sarah Chen',
    timestamp: '2023-06-16 18:42',
    isDecoded: false,
    decodedContent:
      "If you're reading this, I might already be gone. I've hidden the data drive in my personal locker. The code is 4-9-2-7. Whatever you do, don't let them weaponize what we've discovered. It could change humanity forever.",
    requiredProgress: 5,
  },
};

// Function to get a message by ID
export const getMessage = (id: string): Message | undefined => {
  return messages[id];
};

/**
 * Get a decoded message based on the game progress
 * @param idOrMessage The message ID or Message object
 * @param gameProgress The current game progress
 * @returns The decoded message content if available, otherwise the original content
 */
export const getDecodedMessage = (idOrMessage: string | Message, gameProgress: number): string => {
  // If idOrMessage is a string (message ID), get the message object
  const message = typeof idOrMessage === 'string' ? getMessage(idOrMessage) : idOrMessage;
  if (!message) return '';

  // If the message is already decoded or has no decoded content, return the original content
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
  const staticWords = originalWords.filter((word) => word.includes('[static]'));
  const wordsToReplace = Math.floor(staticWords.length * decodeRatio);

  // Create a map of which indices contain static
  const staticIndices = originalWords
    .map((word, index) => (word.includes('[static]') ? index : -1))
    .filter((index) => index !== -1);

  // Randomly select indices to decode
  const indicesToDecode = staticIndices
    .sort(() => Math.random() - 0.5)
    .slice(0, Math.min(wordsToReplace, staticIndices.length));

  // Replace selected words with decoded versions
  const result = [...originalWords];
  indicesToDecode.forEach((index) => {
    result[index] = decodedWords[index];
  });

  return result.join(' ');
};
