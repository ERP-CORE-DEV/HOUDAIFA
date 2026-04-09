import React, { useEffect } from 'react';
import { Modal, Form, Input, Select, message } from 'antd';
import { certificationService } from '../../../services/training';
import { CertificationRncp } from '../../../types/training';

const { Option } = Select;

interface CertificationModalProps {
  visible: boolean;
  onCancel: () => void;
  onSuccess: () => void;
  editRecord?: CertificationRncp | null;
}

type CertificationFormValues = {
  RncpCode: string;
  Titre: string;
  Organisme: string;
  NiveauQualification: string;
  DateEnregistrement: string;
  DateEcheance?: string;
};

const NIVEAU_OPTIONS = [
  { value: 'Niveau 3', label: 'Niveau 3 — CAP / BEP' },
  { value: 'Niveau 4', label: 'Niveau 4 — Baccalaureat' },
  { value: 'Niveau 5', label: 'Niveau 5 — Bac +2 (BTS, BUT, DUT)' },
  { value: 'Niveau 6', label: 'Niveau 6 — Bac +3 / +4 (Licence, Bachelor)' },
  { value: 'Niveau 7', label: 'Niveau 7 — Bac +5 (Master, Ingenieur)' },
  { value: 'Niveau 8', label: 'Niveau 8 — Doctorat' },
];

const CertificationModal: React.FC<CertificationModalProps> = ({
  visible,
  onCancel,
  onSuccess,
  editRecord,
}) => {
  const [form] = Form.useForm<CertificationFormValues>();
  const [saving, setSaving] = React.useState(false);

  useEffect(() => {
    if (!visible) return;
    if (editRecord) {
      form.setFieldsValue({
        RncpCode: editRecord.RncpCode,
        Titre: editRecord.Titre,
        Organisme: editRecord.Organisme,
        NiveauQualification: editRecord.NiveauQualification,
        DateEnregistrement: editRecord.DateEnregistrement
          ? editRecord.DateEnregistrement.substring(0, 10)
          : '',
        DateEcheance: editRecord.DateEcheance
          ? editRecord.DateEcheance.substring(0, 10)
          : undefined,
      });
    } else {
      form.resetFields();
    }
  }, [visible, editRecord, form]);

  const handleOk = async () => {
    const values = await form.validateFields();
    setSaving(true);
    try {
      if (editRecord) {
        await certificationService.update(editRecord.Id, { ...values, IsActive: editRecord.IsActive });
        message.success('Certification RNCP mise a jour');
      } else {
        await certificationService.create({ ...values, IsActive: true });
        message.success('Certification RNCP creee');
      }
      onSuccess();
    } catch {
      message.error('Erreur lors de la sauvegarde de la certification');
    } finally {
      setSaving(false);
    }
  };

  const modalTitle = editRecord ? 'Modifier la certification RNCP' : 'Nouvelle certification RNCP';

  return (
    <Modal
      title={modalTitle}
      open={visible}
      onOk={handleOk}
      onCancel={onCancel}
      okText={editRecord ? 'Mettre a jour' : 'Creer'}
      cancelText="Annuler"
      confirmLoading={saving}
      width={560}
      destroyOnClose
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item
          name="RncpCode"
          label="Code RNCP"
          rules={[
            { required: true, message: 'Le code RNCP est obligatoire' },
            {
              pattern: /^RNCP\d+$/i,
              message: 'Format attendu : RNCP suivi de chiffres (ex. RNCP35634)',
            },
          ]}
        >
          <Input placeholder="Ex. : RNCP35634" />
        </Form.Item>

        <Form.Item
          name="Titre"
          label="Titre de la certification"
          rules={[{ required: true, message: 'Le titre est obligatoire' }]}
        >
          <Input placeholder="Intitule officiel de la certification" />
        </Form.Item>

        <Form.Item
          name="Organisme"
          label="Organisme certificateur"
          rules={[{ required: true, message: "L'organisme est obligatoire" }]}
        >
          <Input placeholder="Ex. : Ministere du Travail, CPNE, organisme prive" />
        </Form.Item>

        <Form.Item
          name="NiveauQualification"
          label="Niveau de qualification (cadre europeen)"
          rules={[{ required: true, message: 'Le niveau est obligatoire' }]}
        >
          <Select placeholder="Selectionner un niveau">
            {NIVEAU_OPTIONS.map((opt) => (
              <Option key={opt.value} value={opt.value}>
                {opt.label}
              </Option>
            ))}
          </Select>
        </Form.Item>

        <Form.Item
          name="DateEnregistrement"
          label="Date d'enregistrement au RNCP"
          rules={[{ required: true, message: "La date d'enregistrement est obligatoire" }]}
        >
          <Input type="date" />
        </Form.Item>

        <Form.Item name="DateEcheance" label="Date d'echeance (renouvellement)">
          <Input type="date" />
        </Form.Item>
      </Form>
    </Modal>
  );
};

export default CertificationModal;
