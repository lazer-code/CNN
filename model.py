import os, cv2, sys
from ultralytics import YOLO

def process_video(path, model):
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

        with open("output.txt", "w") as file:
            file.write(f"Processing: {progress:.2f}% completed")

    with open("output.txt", "w") as file:
        file.write("")
        
    cap.release()
    out.release()
    cv2.destroyAllWindows()

    with open("ModelOutput/output.txt", "w") as file:
        file.write("Processing complete.")

def process_image(path, model):
    img = model(cv2.imread(path))[0].plot()
    os.makedirs("ModelOutput", exist_ok=True)

    id = 1
    output_image_path = "ModelOutput/image.jpg"
    name = "ModelOutput/image"

    while os.path.exists(output_image_path):
        output_image_path = name + str(id) + ".jpg"
        id += 1

    cv2.imwrite(output_image_path, img)

def main(path):
    model = YOLO("C:\\Users\\Shaked\\Documents\\Projects\\CNN\\runs\\detect\\train\\weights\\best.pt")
    
    if path.endswith(('.mp4', '.mov', '.avi', '.mkv')):
        process_video(path, model)
    else:
        process_image(path, model)
        
    sys.exit(0)

if __name__ == "__main__":
    main("".join(sys.argv[1:]))
