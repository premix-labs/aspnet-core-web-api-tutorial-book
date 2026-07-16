export function calculateReadingTime(text: string): number {
  if (!text) return 1;
  const wordsPerMinute = 200;
  // Remove markdown formatting like #, *, [, ], (, )
  const plainText = text.replace(/[#*\[\]()_`~>]/g, ' ');
  const words = plainText.split(/\s+/).filter((word) => word.length > 0);
  const minutes = Math.ceil(words.length / wordsPerMinute);
  return minutes > 0 ? minutes : 1;
}
