import os, sys, cv2, re, yt_dlp, time, socket, glob, shutil
from ultralytics import YOLO

def checkIfYoutube(sock: socket.socket, url: str, save_path="Videos"):
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

def main(path):
    try:
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as sock:
            sock.connect(("127.0.0.1", 12345))
            
            for folder in glob.glob("runs/detect/predict*/"):
                shutil.rmtree(folder)

            vid_path = checkIfYoutube(sock, path)

            if vid_path != "":
                path = vid_path

            cap = cv2.VideoCapture(path)
            top = int(cap.get(cv2.CAP_PROP_FRAME_COUNT))

            model = YOLO(os.path.dirname(os.path.abspath(__file__)) + "\\best.pt")

            for frame_num, result in enumerate(model.predict(source=path, stream=True, save=True)):
                sock.sendall(f"{frame_num + 1} out of {top} frames!".encode())

            path = max((f for f in map(lambda f: os.path.join("runs/detect/predict", f), os.listdir("runs/detect/predict")) if os.path.isfile(f)), key=os.path.getmtime, default=None)

            sock.sendall(("DONE." + os.getcwd() + "\\" + path).encode())
    except Exception as e:
        print()
        print(e)
        time.sleep(1000)
        

if __name__ == "__main__":
    main(sys.argv[1])
