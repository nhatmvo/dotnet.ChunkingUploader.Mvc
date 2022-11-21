# dotnet.ChunkingUploader.Mvc

When trying to upload a large file size from client, the optimal way is to split that file into multiple chunks. There are 2 ways that those chunks can be handled from server and client side:

1. *Synchronous*: when file is splitted into multiple chunks, those chunks will be sent sequentially (1 by 1 right after the prevous chunk sending request process successfully). After receiving all chunks, server will perform merging process.
2. *Asynchronous*: when file is splitted into multiple chunks, those chunks will be sent in "asynchronous" manner (not waiting for previous chunk sending request to finish), which is the appoarch of this "project". After receiving all chunks, server will perform merging process.

Since this project take asynchronous way to handle the split file problem, there're several "small problems" that will come up:
### Client (This should be easy):
- **Client** send download chunk request without waiting the previous send chunk request to be finished - **Appoarch**: split file handle by javascript within the page that client use to choose file from their computers. After splitted, chunk request will be created and sent using Ajax request (luckily Ajax request is async by default)

### Server (The Hard Part):
- Somehow, server should have a mechanism to check if all chunks have been successfully downloaded, so that those chunks can be available for merge process.
- Requests must be handled by concurrent, since Ajax requests are sent by asynchronous manner, which means that server must have a proper locking mechanism when processing with chunks

### Project Appoarch: 
#### Sequence Diagram:

The following diagram dipics the overall flow to handle upload chunk process

![VP31JlCm3CVlVWfhktpVWHT0Q4oQn0iWSVTQTmiuSN4S1czFsenjC44FZUN-_hywNcSdyn85g7Crt4ZWv4WPkxc2hPRKattWK-33r7-h9tIQt5HmqqvXcoDkjGAWuffAtDRkucwx-hTS_by0D27Uh6R5BPPHb7eSWZ0EfX55EnACrEr3OfYWCvn72TxOa5onsvVqadz0e_58naZFz8x6huMp](https://user-images.githubusercontent.com/22346498/201979586-961ac44c-7a65-4439-b886-73efe7107b84.png)

#### Coding:

- Using ConcurrentDictionary to create a temporary storage to check if chunks from file are ready for merge process.
- Using ConcurrentDictionary to create a temporary storage to create locking mechanism for each file.
