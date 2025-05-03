import os, cv2, re, yt_dlp, time, socket
from ultralytics import YOLO

def process_video(sock: socket.socket, path: str, model):
    cap = cv2.VideoCapture(path)
    
    total_frames = int(cap.get(cv2.CAP_PROP_FRAME_COUNT))
    fps = cap.get(cv2.CAP_PROP_FPS)
    frame_size = (int(cap.get(cv2.CAP_PROP_FRAME_WIDTH)), int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT)))
    os.makedirs("ModelOutput", exist_ok=True)
    output_video_path = "ModelOutput/video.mp4"
    
    id = 1
    name = output_video_path[:-4]
    while os.path.exists(output_video_path):
        output_video_path = name + str(id) + output_video_path[-4:]
        id += 1

    out = cv2.VideoWriter(output_video_path, cv2.VideoWriter_fourcc(*'mp4v'), fps, frame_size)

    frame_idx = 0

    while cap.isOpened():
        ret, frame = cap.read()
        if not ret:
            break
        img = model(frame, verbose=False)[0].plot()
        out.write(cv2.resize(img, frame_size))
        
        frame_idx += 1
        progress = (frame_idx / total_frames) * 100
        sock.sendall(f"Processing: {progress:.2f}% completed".encode())
        
    cap.release()
    out.release()
    cv2.destroyAllWindows()

    sock.sendall("Processing ended.".encode())
    return output_video_path

def checkIfYoutube(sock: socket.socket, url: str, save_path="ModelOutput"):
    if re.match(r"(https?://)?(www\.)?(youtube\.com|youtu\.be)/.+", url):
        os.makedirs(save_path, exist_ok=True)

        sock.sendall("Downloading process started.\n".encode())

        output_file = os.path.join(save_path, "video.mp4")

        id = 1
        while os.path.exists(output_file):
            output_file = os.path.join(save_path, f"video{id}.mp4")
            id += 1

        def log_progress(d):
            per = str(d['_percent'])
            sock.sendall((f"Downloading: {per}%").encode())

        ydl_opts = {
            "outtmpl": output_file,
            "format": "bestvideo",
            "quiet": True,
            "progress_hooks": [log_progress],
        }

        try:
            with yt_dlp.YoutubeDL(ydl_opts) as ydl:
                ydl.download([url])

            sock.sendall(("Video downloaded.").encode())

            return output_file
        
        except PermissionError as e:
            sock.sendall("Error" + str(e))
            checkIfYoutube(url)

        except Exception as e:
            sock.sendall("Error" + str(e))

    return ""

def process_image(sock: socket.socket, path: str, model):
    img = model(cv2.imread(path))[0].plot()
    os.makedirs("ModelOutput", exist_ok=True)

    id = 1
    output_image_path = "ModelOutput/image.jpg"
    name = "ModelOutput/image"

    while os.path.exists(output_image_path):
        output_image_path = name + str(id) + ".jpg"
        id += 1

    cv2.imwrite(output_image_path, img)
    return output_image_path

def main():
    model = YOLO("best.pt")

    time.sleep(5)
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as sock:
        sock.connect(("127.0.0.1", 12345))

        path = sock.recv(1024).decode()

        vid_path = checkIfYoutube(sock, path)

        if vid_path != "":
            path = vid_path

        if path.endswith(('.mp4', '.mov', '.avi', '.mkv')):
            path = process_video(sock, path, model)
        else:
            path = process_image(sock, path, model)

        sock.sendall(("DONE." + os.getcwd() + "\\" + path).encode())
        

if __name__ == "__main__":
    main()
