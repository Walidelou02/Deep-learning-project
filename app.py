import io
from flask import Flask, request, jsonify
import numpy as np
import tensorflow as tf
from PIL import Image

# Load your model (replace with the correct path to your trained model)
model = tf.keras.models.load_model('final_model.keras')

# Dictionary of labels (same as in your script)
labels = {
    'Apple___Apple_scab': 0, 'Apple___Black_rot': 1, 'Apple___Cedar_apple_rust': 2, 'Apple___healthy': 3,
    'Blueberry___healthy': 4, 'Cherry_(including_sour)___Powdery_mildew': 5, 'Cherry_(including_sour)___healthy': 6,
    'Corn_(maize)___Cercospora_leaf_spot Gray_leaf_spot': 7, 'Corn_(maize)___Common_rust_': 8,
    'Corn_(maize)___Northern_Leaf_Blight': 9, 'Corn_(maize)___healthy': 10, 'Grape___Black_rot': 11,
    'Grape___Esca_(Black_Measles)': 12, 'Grape___Leaf_blight_(Isariopsis_Leaf_Spot)': 13, 'Grape___healthy': 14,
    'Orange___Haunglongbing_(Citrus_greening)': 15, 'Peach___Bacterial_spot': 16, 'Peach___healthy': 17,
    'Pepper,_bell___Bacterial_spot': 18, 'Pepper,_bell___healthy': 19, 'Potato___Early_blight': 20,
    'Potato___Late_blight': 21, 'Potato___healthy': 22, 'Raspberry___healthy': 23, 'Soybean___healthy': 24,
    'Squash___Powdery_mildew': 25, 'Strawberry___Leaf_scorch': 26, 'Strawberry___healthy': 27,
    'Tomato___Bacterial_spot': 28, 'Tomato___Early_blight': 29, 'Tomato___Late_blight': 30, 'Tomato___Leaf_Mold': 31,
    'Tomato___Septoria_leaf_spot': 32, 'Tomato___Spider_mites Two-spotted_spider_mite': 33, 'Tomato___Target_Spot': 34,
    'Tomato___Tomato_Yellow_Leaf_Curl_Virus': 35, 'Tomato___Tomato_mosaic_virus': 36, 'Tomato___healthy': 37
}

# Initialize Flask app
app = Flask(__name__)

# API endpoint for prediction
@app.route('/predict', methods=['POST'])
def predict():
    if 'file' not in request.files:
        return jsonify({'error': 'No file part'}), 400

    file = request.files['file']
    if file.filename == '':
        return jsonify({'error': 'No selected file'}), 400

    try:
        # Read the image file into an in-memory byte stream
        img_bytes = file.read()

        try:
            # Attempt to open the image from the byte stream
            img = Image.open(io.BytesIO(img_bytes))
            img = img.convert("RGB")  # Ensure the image is in RGB format if necessary

            # Resize the image and process it with Keras
            img = img.resize((224, 224))  # Resize the image here directly
            img_array = np.array(img) / 255.0  # Convert to array and normalize
            img_array = np.expand_dims(img_array, axis=0)  # Add batch dimension

        except Exception as e:
            return jsonify({'error': f"Invalid image file: {str(e)}"}), 400

        # Make a prediction
        predictions = model.predict(img_array)

        # Get the predicted class
        predicted_class_idx = np.argmax(predictions)
        predicted_label = list(labels.keys())[list(labels.values()).index(predicted_class_idx)]

        return jsonify({'predicted_class': predicted_label, 'prediction_probabilities': predictions.tolist()})

    except Exception as e:
        return jsonify({'error': f"Error during prediction: {str(e)}"}), 500


if __name__ == '__main__':
    app.run(debug=True)
