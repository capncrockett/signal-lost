declare module 'pngjs' {
  export class PNG {
    width: number;
    height: number;
    data: Buffer;

    constructor(options?: {
      width?: number;
      height?: number;
      checkCRC?: boolean;
      deflateChunkSize?: number;
      deflateLevel?: number;
      deflateStrategy?: number;
      filterType?: number;
    });

    static sync: {
      read(buffer: Buffer): PNG;
      write(png: PNG): Buffer;
    };

    pack(): PNG;
    parse(data: Buffer, callback?: (error: Error | null, data: PNG) => void): PNG;
    write(path: string, callback?: (error: Error | null) => void): void;
  }
}
