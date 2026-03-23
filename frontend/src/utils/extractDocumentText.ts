import { GlobalWorkerOptions, getDocument } from "pdfjs-dist";

// Use the bundled worker
GlobalWorkerOptions.workerSrc = new URL(
  "pdfjs-dist/build/pdf.worker.min.mjs",
  import.meta.url
).toString();

/**
 * Extract text content from a PDF file buffer.
 */
async function extractPdfText(buffer: ArrayBuffer): Promise<string> {
  const pdf = await getDocument({ data: buffer }).promise;
  const pages: string[] = [];

  for (let i = 1; i <= pdf.numPages; i++) {
    const page = await pdf.getPage(i);
    const content = await page.getTextContent();
    const text = content.items
      .filter((item): item is { str: string } & typeof item => "str" in item)
      .map((item) => item.str)
      .join(" ");
    pages.push(text);
  }

  return pages.join("\n\n");
}

/**
 * Extract text from a .pdf or .md/.txt file.
 * Returns the extracted text string.
 */
export async function extractDocumentText(file: File): Promise<string> {
  const ext = file.name.split(".").pop()?.toLowerCase();

  if (ext === "pdf") {
    const buffer = await file.arrayBuffer();
    return extractPdfText(buffer);
  }

  // MD, TXT, or any other text-based file
  return file.text();
}
