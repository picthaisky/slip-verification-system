import React, { useState } from 'react';
import { View, StyleSheet, Image, ScrollView, Alert } from 'react-native';
import { Text, Card, Button, TextInput, ProgressBar, useTheme } from 'react-native-paper';
import { launchCamera, launchImageLibrary } from 'react-native-image-picker';
import Icon from 'react-native-vector-icons/MaterialCommunityIcons';
import { slipApi } from '../../api/endpoints/slip';
import { t } from '../../locales';

const SlipUploadScreen = ({ navigation }: any) => {
  const theme = useTheme();
  const [selectedImage, setSelectedImage] = useState<any>(null);
  const [orderId, setOrderId] = useState('');
  const [uploading, setUploading] = useState(false);
  const [uploadProgress, setUploadProgress] = useState(0);
  const [result, setResult] = useState<any>(null);

  const handleTakePhoto = () => {
    launchCamera(
      {
        mediaType: 'photo',
        quality: 0.8,
        saveToPhotos: false,
      },
      (response) => {
        if (response.didCancel) {
          console.log('User cancelled camera');
        } else if (response.errorCode) {
          Alert.alert('Error', response.errorMessage || 'Camera error');
        } else if (response.assets && response.assets[0]) {
          setSelectedImage(response.assets[0]);
        }
      }
    );
  };

  const handleChooseFromGallery = () => {
    launchImageLibrary(
      {
        mediaType: 'photo',
        quality: 0.8,
      },
      (response) => {
        if (response.didCancel) {
          console.log('User cancelled image picker');
        } else if (response.errorCode) {
          Alert.alert('Error', response.errorMessage || 'Image picker error');
        } else if (response.assets && response.assets[0]) {
          setSelectedImage(response.assets[0]);
        }
      }
    );
  };

  const handleUpload = async () => {
    if (!selectedImage || !orderId) {
      Alert.alert('Error', t('errors.validationError'));
      return;
    }

    setUploading(true);
    setUploadProgress(0);
    setResult(null);

    try {
      const uploadedSlip = await slipApi.uploadSlip(
        orderId,
        selectedImage,
        (progress) => {
          setUploadProgress(progress);
        }
      );

      setResult(uploadedSlip);
      Alert.alert('Success', t('slip.uploadSuccess'));
      
      // Reset form after 2 seconds
      setTimeout(() => {
        setSelectedImage(null);
        setOrderId('');
        setResult(null);
        setUploadProgress(0);
      }, 2000);
    } catch (error: any) {
      Alert.alert('Error', error.response?.data?.message || t('slip.uploadError'));
    } finally {
      setUploading(false);
    }
  };

  const handleRemoveImage = () => {
    setSelectedImage(null);
  };

  return (
    <ScrollView style={styles.container} contentContainerStyle={styles.content}>
      <View style={styles.header}>
        <Text variant="headlineMedium" style={styles.title}>
          {t('slip.uploadSlip')}
        </Text>
      </View>

      {!selectedImage ? (
        <Card style={styles.card}>
          <Card.Content>
            <Text variant="titleMedium" style={styles.sectionTitle}>
              {t('slip.selectImage')}
            </Text>
            <Button
              mode="contained"
              icon="camera"
              onPress={handleTakePhoto}
              style={styles.button}
            >
              {t('slip.takePhoto')}
            </Button>
            <Button
              mode="outlined"
              icon="image"
              onPress={handleChooseFromGallery}
              style={styles.button}
            >
              {t('slip.chooseFromGallery')}
            </Button>
          </Card.Content>
        </Card>
      ) : (
        <>
          <Card style={styles.card}>
            <Card.Content>
              <Text variant="titleMedium" style={styles.sectionTitle}>
                {t('slip.preview')}
              </Text>
              {selectedImage.uri && (
                <Image
                  source={{ uri: selectedImage.uri }}
                  style={styles.previewImage}
                  resizeMode="contain"
                />
              )}
              <Button
                mode="outlined"
                icon="close"
                onPress={handleRemoveImage}
                style={styles.button}
              >
                {t('common.delete')}
              </Button>
            </Card.Content>
          </Card>

          <Card style={styles.card}>
            <Card.Content>
              <TextInput
                label={t('slip.orderId')}
                value={orderId}
                onChangeText={setOrderId}
                mode="outlined"
                left={<TextInput.Icon icon="tag" />}
                style={styles.input}
              />

              {uploading && (
                <View style={styles.progressContainer}>
                  <Text variant="bodySmall" style={styles.progressText}>
                    {t('slip.uploading')} {uploadProgress}%
                  </Text>
                  <ProgressBar
                    progress={uploadProgress / 100}
                    color={theme.colors.primary}
                    style={styles.progressBar}
                  />
                </View>
              )}

              <Button
                mode="contained"
                icon="upload"
                onPress={handleUpload}
                loading={uploading}
                disabled={uploading || !orderId}
                style={styles.button}
              >
                {t('slip.upload')}
              </Button>
            </Card.Content>
          </Card>

          {result && (
            <Card style={styles.card}>
              <Card.Content>
                <View style={styles.resultHeader}>
                  <Icon name="check-circle" size={48} color="#4CAF50" />
                  <Text variant="titleLarge" style={styles.resultTitle}>
                    {t('slip.verificationResult')}
                  </Text>
                </View>
                <View style={styles.resultItem}>
                  <Text variant="bodyMedium" style={styles.resultLabel}>
                    {t('slip.amount')}:
                  </Text>
                  <Text variant="bodyMedium" style={styles.resultValue}>
                    ฿{result.amount?.toFixed(2)}
                  </Text>
                </View>
                <View style={styles.resultItem}>
                  <Text variant="bodyMedium" style={styles.resultLabel}>
                    {t('slip.bankName')}:
                  </Text>
                  <Text variant="bodyMedium" style={styles.resultValue}>
                    {result.bankName}
                  </Text>
                </View>
                <View style={styles.resultItem}>
                  <Text variant="bodyMedium" style={styles.resultLabel}>
                    {t('slip.status')}:
                  </Text>
                  <Text variant="bodyMedium" style={styles.resultValue}>
                    {result.status}
                  </Text>
                </View>
              </Card.Content>
            </Card>
          )}
        </>
      )}
    </ScrollView>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  content: {
    padding: 16,
  },
  header: {
    marginBottom: 16,
  },
  title: {
    fontWeight: 'bold',
  },
  card: {
    marginBottom: 16,
  },
  sectionTitle: {
    marginBottom: 16,
    fontWeight: 'bold',
  },
  button: {
    marginVertical: 6,
  },
  previewImage: {
    width: '100%',
    height: 300,
    borderRadius: 8,
    marginBottom: 16,
  },
  input: {
    marginBottom: 16,
  },
  progressContainer: {
    marginBottom: 16,
  },
  progressText: {
    marginBottom: 8,
    textAlign: 'center',
  },
  progressBar: {
    height: 8,
    borderRadius: 4,
  },
  resultHeader: {
    alignItems: 'center',
    marginBottom: 16,
  },
  resultTitle: {
    marginTop: 8,
    fontWeight: 'bold',
  },
  resultItem: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    paddingVertical: 8,
    borderBottomWidth: 1,
    borderBottomColor: '#E0E0E0',
  },
  resultLabel: {
    fontWeight: '500',
  },
  resultValue: {
    color: '#666',
  },
});

export default SlipUploadScreen;
